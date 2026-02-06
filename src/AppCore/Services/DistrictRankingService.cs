namespace AppCore.Services;

using AppCore.Abstractions.Aggregation;
using AppCore.Abstractions.DataFetching;
using AppCore.Abstractions.Leaderboard;
using AppCore.Abstractions.Persistence;
using AppCore.Abstractions.Services;
using AppCore.Models;

public sealed class DistrictRankingService : IDistrictRankingService
{
    private readonly IDataFetchingService _dataFetchingService;
    private readonly IWeatherAggregationService _aggregationService;
    private readonly IWeatherSnapshotRepository _snapshotRepository;
    private readonly ILeaderboardStore _leaderboardStore;

    public DistrictRankingService(
        IDataFetchingService dataFetchingService,
        IWeatherAggregationService aggregationService,
        IWeatherSnapshotRepository snapshotRepository,
        ILeaderboardStore leaderboardStore)
    {
        _dataFetchingService = dataFetchingService;
        _aggregationService = aggregationService;
        _snapshotRepository = snapshotRepository;
        _leaderboardStore = leaderboardStore;
    }

    public async Task RefreshLeaderboardAsync(CancellationToken cancellationToken)
    {
        // NOTE:
        // District list source will be injected later (DB or config)
        // For now assume districts are already available
        var districts = await GetDistrictsAsync(cancellationToken);

        var snapshots = new List<DistrictWeatherSnapshot>();

        foreach (var district in districts)
        {
            var weather = await _dataFetchingService.GetWeatherForecastAsync(
                district.Latitude,
                district.Longitude,
                cancellationToken);

            var airQuality = await _dataFetchingService.GetAirQualityForecastAsync(
                district.Latitude,
                district.Longitude,
                cancellationToken);

            var snapshot = _aggregationService.Aggregate(
                district,
                weather,
                airQuality);

            snapshots.Add(snapshot);
        }

        await _snapshotRepository.SaveAsync(snapshots, cancellationToken);

        var rankedDistricts = snapshots
            .Join(
                districts,
                s => s.DistrictId,
                d => d.Id,
                (s, d) => new
                {
                    d.Id,
                    d.Name,
                    s.Temp2Pm,
                    s.Pm25_2Pm
                })
            .OrderBy(x => x.Temp2Pm)
            .ThenBy(x => x.Pm25_2Pm)
            .Select((x, index) => new RankedDistrict(
                x.Id,
                x.Name,
                x.Temp2Pm,
                x.Pm25_2Pm,
                index + 1))
            .ToList();

        await _leaderboardStore.StoreAsync(rankedDistricts, cancellationToken);
    }

    public async Task<IReadOnlyCollection<RankedDistrict>> GetTopDistrictsAsync(
        int count,
        CancellationToken cancellationToken)
    {
        var results = await _leaderboardStore.GetTopDistrictsAsync(count, cancellationToken);

        if (results.Count == 0)
        {
            throw new InvalidOperationException("Leaderboard data not ready.");
        }

        return results;
    }

    private static Task<IReadOnlyCollection<District>> GetDistrictsAsync(
        CancellationToken cancellationToken)
    {
        // Placeholder for now.
        // This will later be backed by DB or cached data.
        throw new NotImplementedException("District source not wired yet.");
    }
}