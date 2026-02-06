namespace Infrastructure.DataFetching.OpenMeteo;

using System.Globalization;
using System.Net.Http.Json;
using AppCore.Abstractions.DataFetching;
using AppCore.Models;

public sealed class OpenMeteoDataFetchingService : IDataFetchingService
{
    private readonly HttpClient _httpClient;

    public OpenMeteoDataFetchingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RawWeatherForecast> GetWeatherForecastAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        var url =
            $"forecast?latitude={latitude}&longitude={longitude}" +
            $"&hourly=temperature_2m&timezone=UTC";

        var response =
            await _httpClient.GetFromJsonAsync<OpenMeteoWeatherResponse>(
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
            $"&hourly=pm2_5&timezone=UTC";

        var response =
            await _httpClient.GetFromJsonAsync<OpenMeteoAirQualityResponse>(
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
}