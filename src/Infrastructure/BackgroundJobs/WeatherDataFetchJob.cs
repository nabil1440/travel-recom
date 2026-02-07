namespace Infrastructure.BackgroundJobs;

using AppCore.Abstractions.DataFetching;
using AppCore.Abstractions.Events;
using AppCore.Abstractions.Services;
using AppCore.Events;
using AppCore.Models;
using Infrastructure.Jobs;
using Microsoft.Extensions.Logging;

public sealed class WeatherDataFetchJob : IWeatherDataFetchJob
{
  private const int TargetUtcHour = 8; // 2 PM GMT+6 → 08:00 UTC

  private readonly IDistrictService _districtService;
  private readonly IDataFetchingService _dataFetchingService;
  private readonly IEventPublisher _eventPublisher;
  private readonly ILogger<WeatherDataFetchJob> _logger;

  public WeatherDataFetchJob(
      IDistrictService districtService,
      IDataFetchingService dataFetchingService,
      IEventPublisher eventPublisher,
      ILogger<WeatherDataFetchJob> logger)
  {
    _districtService = districtService;
    _dataFetchingService = dataFetchingService;
    _eventPublisher = eventPublisher;
    _logger = logger;
  }

  public async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    var districts = await _districtService.GetDistrictsAsync(cancellationToken);

    _logger.LogInformation("Weather data fetch started for {Count} districts", districts.Count);

    var tasks = districts.Select(d => FetchForDistrictAsync(d, cancellationToken));
    var results = await Task.WhenAll(tasks);

    var districtFacts = results
        .Where(r => r is not null)
        .Cast<DistrictWeatherFacts>()
        .ToList();

    if (districtFacts.Count == 0)
    {
      _logger.LogWarning("No weather data fetched for any district");
      return;
    }

    var @event = new WeatherDataBatchFetched(
        FetchedAtUtc: DateTime.UtcNow,
        Districts: districtFacts,
        BatchId: Guid.NewGuid().ToString("N"));

    await _eventPublisher.PublishAsync(@event, cancellationToken);

    _logger.LogInformation(
        "Published WeatherDataBatchFetched with {Count} districts",
        districtFacts.Count);
  }

  private async Task<DistrictWeatherFacts?> FetchForDistrictAsync(
      District district,
      CancellationToken cancellationToken)
  {
    try
    {
      var weatherTask = _dataFetchingService.GetWeatherForecastAsync(
          district.Latitude, district.Longitude, cancellationToken);

      var airQualityTask = _dataFetchingService.GetAirQualityForecastAsync(
          district.Latitude, district.Longitude, cancellationToken);

      await Task.WhenAll(weatherTask, airQualityTask);

      var weather = weatherTask.Result;
      var airQuality = airQualityTask.Result;

      var tempByDate = Extract2PmValues(weather.Timestamps, weather.Temperatures);
      var pm25ByDate = Extract2PmValues(airQuality.Timestamps, airQuality.Pm25Values);

      var commonDates = tempByDate.Keys
          .Intersect(pm25ByDate.Keys)
          .OrderBy(d => d)
          .ToList();

      if (commonDates.Count == 0)
      {
        _logger.LogWarning(
            "No 2PM data points for district {DistrictId} ({Name})",
            district.Id, district.Name);
        return null;
      }

      var forecasts = commonDates
          .Select(date => new DailyWeatherFact(
              date,
              tempByDate[date],
              pm25ByDate[date]))
          .ToList();

      return new DistrictWeatherFacts(district.Id, forecasts);
    }
    catch (Exception ex)
    {
      _logger.LogWarning(
          ex,
          "Failed to fetch weather data for district {DistrictId} ({Name})",
          district.Id, district.Name);
      return null;
    }
  }

  private static Dictionary<DateOnly, double> Extract2PmValues(
      IReadOnlyList<DateTime> timestamps,
      IReadOnlyList<double> values)
  {
    var result = new Dictionary<DateOnly, double>();

    for (var i = 0; i < timestamps.Count; i++)
    {
      if (timestamps[i].Hour != TargetUtcHour)
        continue;

      var date = DateOnly.FromDateTime(timestamps[i]);

      if (!result.ContainsKey(date))
      {
        result[date] = values[i];
      }
    }

    return result;
  }
}
