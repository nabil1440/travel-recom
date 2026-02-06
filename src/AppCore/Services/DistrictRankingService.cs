namespace AppCore.Services;

using AppCore.Abstractions.Aggregation;
using AppCore.Abstractions.DataFetching;
using AppCore.Abstractions.Leaderboard;
using AppCore.Abstractions.Persistence;
using AppCore.Abstractions.Services;
using AppCore.Models;
using Microsoft.Extensions.Logging;

public sealed class DistrictRankingService : IDistrictRankingService
{
  private readonly IDataFetchingService _dataFetchingService;
  private readonly IWeatherAggregationService _aggregationService;
  private readonly IWeatherSnapshotRepository _snapshotRepository;
  private readonly ILeaderboardStore _leaderboardStore;
  private readonly IDistrictService _districtService;
  private readonly ILogger<DistrictRankingService> _logger;

  public DistrictRankingService(
      IDataFetchingService dataFetchingService,
      IWeatherAggregationService aggregationService,
      IWeatherSnapshotRepository snapshotRepository,
      IDistrictService districtService,
      ILeaderboardStore leaderboardStore,
      ILogger<DistrictRankingService> logger
  )
  {
    _dataFetchingService = dataFetchingService;
    _aggregationService = aggregationService;
    _snapshotRepository = snapshotRepository;
    _leaderboardStore = leaderboardStore;
    _districtService = districtService;
    _logger = logger;
  }

  public async Task RefreshLeaderboardAsync(CancellationToken cancellationToken)
  {
    var districts = await GetDistrictsAsync(cancellationToken);

    if (districts.Count == 0)
    {
      return;
    }

    // Build lookup once
    var districtMap = districts.ToDictionary(d => d.Id);

    // Parallel fetch + aggregation
    var tasks = districts.Select(d => FetchAndAggregateAsync(d, cancellationToken));

    var results = await Task.WhenAll(tasks);

    var snapshots = results.Where(s => s is not null).Cast<DistrictWeatherSnapshot>().ToList();

    if (snapshots.Count == 0)
    {
      // No successful updates, keep existing leaderboard
      return;
    }

    // Persist raw snapshots (source of truth)
    await _snapshotRepository.SaveAsync(snapshots, cancellationToken);

    // Rank + enrich with district metadata
    var rankedDistricts = snapshots
        .Where(s => districtMap.ContainsKey(s.DistrictId))
        .OrderBy(s => s.Temp2Pm)
        .ThenBy(s => s.Pm25_2Pm)
        .Select(
            (s, index) =>
            {
              var district = districtMap[s.DistrictId];

              return new RankedDistrict(
                      s.DistrictId,
                      district.Name,
                      s.Temp2Pm,
                      s.Pm25_2Pm,
                      index + 1
                  );
            }
        )
        .ToList();

    // Store leaderboard projection (Redis ZSET)
    await _leaderboardStore.StoreAsync(rankedDistricts, cancellationToken);
  }

  public async Task<IReadOnlyCollection<RankedDistrict>> GetTopDistrictsAsync(
      int count,
      CancellationToken cancellationToken
  )
  {
    var results = await _leaderboardStore.GetTopDistrictsAsync(count, cancellationToken);

    if (results.Count == 0)
    {
      throw new InvalidOperationException("Leaderboard data not ready.");
    }

    return results;
  }

  private async Task<IReadOnlyCollection<District>> GetDistrictsAsync(
      CancellationToken cancellationToken
  )
  {
    var districts = await _districtService.GetDistrictsAsync(cancellationToken);
    return districts;
  }

  private async Task<DistrictWeatherSnapshot?> FetchAndAggregateAsync(
      District district,
      CancellationToken cancellationToken
  )
  {
    try
    {
      var weatherTask = _dataFetchingService.GetWeatherForecastAsync(
          district.Latitude,
          district.Longitude,
          cancellationToken
      );

      var airQualityTask = _dataFetchingService.GetAirQualityForecastAsync(
          district.Latitude,
          district.Longitude,
          cancellationToken
      );

      await Task.WhenAll(weatherTask, airQualityTask);

      return _aggregationService.Aggregate(
          district,
          weatherTask.Result,
          airQualityTask.Result
      );
    }
    catch (Exception ex)
    {
      _logger.LogWarning(
          ex,
          "Failed to refresh weather data for district {DistrictId} ({DistrictName})",
          district.Id,
          district.Name
      );

      return null;
    }
  }
}
