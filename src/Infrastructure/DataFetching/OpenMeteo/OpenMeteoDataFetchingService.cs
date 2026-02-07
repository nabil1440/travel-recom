namespace Infrastructure.DataFetching.OpenMeteo;

using System.Globalization;
using System.Net;
using System.Text.Json;
using AppCore.Abstractions.DataFetching;
using AppCore.Models;

public sealed class OpenMeteoDataFetchingService : IDataFetchingService
{
    private const int MaxRetries = 4;
    private static readonly TimeSpan BaseDelay = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(5);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _weatherClient;
    private readonly HttpClient _airQualityClient;

    public OpenMeteoDataFetchingService(IHttpClientFactory factory)
    {
        _weatherClient = factory.CreateClient("OpenMeteoWeather");
        _airQualityClient = factory.CreateClient("OpenMeteoAirQuality");
    }

    public async Task<RawWeatherForecast> GetWeatherForecastAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        var url =
            $"forecast?latitude={latitude}&longitude={longitude}" +
            $"&hourly=temperature_2m&timezone=UTC";

        var response = await GetFromJsonWithRetryAsync<OpenMeteoWeatherResponse>(
            _weatherClient,
            url,
            cancellationToken);

        if (response?.Hourly is null)
            throw new InvalidOperationException("Invalid weather response");

        return new RawWeatherForecast(
            ParseTimestamps(response.Hourly.Time),
            response.Hourly.Temperature_2m
        );
    }

    public async Task<RawAirQualityForecast> GetAirQualityForecastAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        var url =
            $"air-quality?latitude={latitude}&longitude={longitude}" +
            $"&hourly=pm2_5&timezone=UTC&forecast_days=7";

        var response = await GetFromJsonWithRetryAsync<OpenMeteoAirQualityResponse>(
            _airQualityClient,
            url,
            cancellationToken);

        if (response?.Hourly is null)
            throw new InvalidOperationException("Invalid air quality response");

        return new RawAirQualityForecast(
            ParseTimestamps(response.Hourly.Time),
            response.Hourly.Pm2_5
        );
    }

    private static IReadOnlyList<DateTime> ParseTimestamps(
        IReadOnlyList<string> timestamps)
    {
        return timestamps
            .Select(t =>
                DateTime.SpecifyKind(
                    DateTime.Parse(
                        t,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal),
                    DateTimeKind.Utc))
            .ToList();
    }

    private static async Task<T?> GetFromJsonWithRetryAsync<T>(
        HttpClient client,
        string url,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return await JsonSerializer.DeserializeAsync<T>(
                    contentStream,
                    JsonOptions,
                    cancellationToken);
            }

            if (!ShouldRetry(response.StatusCode) || attempt >= MaxRetries)
            {
                response.EnsureSuccessStatusCode();
            }

            var delay = GetRetryDelay(response, attempt);
            await Task.Delay(delay, cancellationToken);
        }

        return default;
    }

    private static bool ShouldRetry(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.TooManyRequests
               || statusCode == HttpStatusCode.RequestTimeout
               || (int)statusCode >= 500;
    }

    private static TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt)
    {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter is not null)
        {
            if (retryAfter.Delta.HasValue)
            {
                return retryAfter.Delta.Value;
            }

            if (retryAfter.Date.HasValue)
            {
                var delay = retryAfter.Date.Value - DateTimeOffset.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    return delay;
                }
            }
        }

        var backoffMs = BaseDelay.TotalMilliseconds * Math.Pow(2, attempt);
        var jitterMs = Random.Shared.Next(0, 200);
        var delayMs = Math.Min(backoffMs + jitterMs, MaxDelay.TotalMilliseconds);

        return TimeSpan.FromMilliseconds(delayMs);
    }
}