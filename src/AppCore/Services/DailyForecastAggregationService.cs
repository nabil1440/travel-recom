namespace AppCore.Services;

using AppCore.Abstractions.Persistence;
using AppCore.Abstractions.Services;
using AppCore.Events;
using AppCore.Models;
using Microsoft.Extensions.Logging;

public sealed class DailyForecastAggregationService : IDailyForecastAggregationService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromDays(8);
    private static readonly TimeSpan LeaderTtl = TimeSpan.FromMinutes(10);
    private const string LockName = "daily-forecast-aggregation";

    private readonly IDailyDistrictForecastRepository _forecastRepository;
    private readonly IDailyForecastCache _cache;
    private readonly IDistrictService _districtService;
    private readonly ILeaderElectionService _leaderElection;
    private readonly ILogger<DailyForecastAggregationService> _logger;

    public DailyForecastAggregationService(
        IDailyDistrictForecastRepository forecastRepository,
        IDailyForecastCache cache,
        IDistrictService districtService,
        ILeaderElectionService leaderElection,
        ILogger<DailyForecastAggregationService> logger)
    {
        _forecastRepository = forecastRepository;
        _cache = cache;
        _districtService = districtService;
        _leaderElection = leaderElection;
        _logger = logger;
    }

    public async Task HandleWeatherBatchAsync(
        WeatherDataBatchFetched @event,
        CancellationToken cancellationToken)
    {
        var acquired = await _leaderElection.TryAcquireAsync(
            LockName,
            LeaderTtl,
            cancellationToken);

        if (!acquired)
        {
            _logger.LogInformation(
                "DailyForecastAggregationService skipped: not leader for batch {BatchId}",
                @event.BatchId);
            return;
        }

        _logger.LogInformation(
            "DailyForecastAggregationService processing batch {BatchId} with {Count} districts",
            @event.BatchId, @event.Districts.Count);

        var forecasts = @event.Districts
            .SelectMany(d => d.Forecasts.Select(f => new DailyDistrictForecast(
                d.DistrictId,
                f.Date,
                f.Temp2Pm,
                f.Pm25_2Pm)))
            .ToList();

        if (forecasts.Count == 0)
        {
            _logger.LogWarning("No forecasts to persist in batch {BatchId}", @event.BatchId);
            return;
        }

        await _forecastRepository.SaveAsync(forecasts, cancellationToken);

        _logger.LogInformation(
            "Persisted {Count} daily forecasts for batch {BatchId}",
            forecasts.Count, @event.BatchId);

        await HydrateCacheAsync(@event, cancellationToken);
    }

    private async Task HydrateCacheAsync(
        WeatherDataBatchFetched @event,
        CancellationToken cancellationToken)
    {
        try
        {
            var districts = await _districtService.GetDistrictsAsync(cancellationToken);
            var districtMap = districts.ToDictionary(d => d.Id);
            var cacheItems = new List<DailyForecastCacheItem>();

            foreach (var districtFacts in @event.Districts)
            {
                if (!districtMap.TryGetValue(districtFacts.DistrictId, out var district))
                    continue;

                foreach (var forecast in districtFacts.Forecasts)
                {
                    cacheItems.Add(new DailyForecastCacheItem(
                        districtFacts.DistrictId,
                        district.Name,
                        forecast.Date,
                        forecast.Temp2Pm,
                        forecast.Pm25_2Pm));
                }
            }

            if (cacheItems.Count > 0)
            {
                await _cache.SetManyAsync(cacheItems, CacheTtl, cancellationToken);
            }

            _logger.LogInformation("Cache hydrated for batch {BatchId}", @event.BatchId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to hydrate cache for batch {BatchId} (non-fatal)",
                @event.BatchId);
        }
    }
}
