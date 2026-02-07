namespace AppCore.Services;

using AppCore.Abstractions.Leaderboard;
using AppCore.Abstractions.Persistence;
using AppCore.Abstractions.Services;
using AppCore.Events;
using AppCore.Models;
using Microsoft.Extensions.Logging;

public sealed class DistrictRankingService : IDistrictRankingService
{
  private readonly IWeatherSnapshotRepository _snapshotRepository;
  private readonly ILeaderboardStore _leaderboardStore;
  private readonly IDistrictService _districtService;
  private readonly ILogger<DistrictRankingService> _logger;

  public DistrictRankingService(
      IWeatherSnapshotRepository snapshotRepository,
      IDistrictService districtService,
      ILeaderboardStore leaderboardStore,
      ILogger<DistrictRankingService> logger
  )
  {
    _snapshotRepository = snapshotRepository;
    _leaderboardStore = leaderboardStore;
    _districtService = districtService;
    _logger = logger;
  }

  public async Task HandleWeatherBatchAsync(
      WeatherDataBatchFetched @event,
      CancellationToken cancellationToken)
  {
    _logger.LogInformation(
        "DistrictRankingService processing batch {BatchId} with {Count} districts",
        @event.BatchId, @event.Districts.Count);

    var snapshots = @event.Districts
        .Where(d => d.Forecasts.Count > 0)
        .Select(d =>
        {
          var avgTemp = d.Forecasts.Average(f => f.Temp2Pm);
          var avgPm25 = d.Forecasts.Average(f => f.Pm25_2Pm);
          var aggregationDate = d.Forecasts.Min(f => f.Date);

          return new DistrictWeatherSnapshot(
              d.DistrictId,
              aggregationDate,
              avgTemp,
              avgPm25);
        })
        .ToList();

    if (snapshots.Count == 0)
    {
      _logger.LogWarning("No valid snapshots to rank in batch {BatchId}", @event.BatchId);
      return;
    }

    // Persist raw snapshots (source of truth)
    await _snapshotRepository.SaveAsync(snapshots, cancellationToken);

    // Rank + enrich with district metadata
    var districts = await GetDistrictsAsync(cancellationToken);
    var districtMap = districts.ToDictionary(d => d.Id);

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

    _logger.LogInformation(
      "DistrictRankingService completed: {Count} districts ranked for batch {BatchId}",
      rankedDistricts.Count, @event.BatchId);
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
}
