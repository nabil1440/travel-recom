namespace Infrastructure.Caching;

using System.Text.Json;
using AppCore.Abstractions.Leaderboard;
using AppCore.Models;
using StackExchange.Redis;

public sealed class RedisLeaderboardStore : ILeaderboardStore
{
    private const string RankKey = "leaderboard:rank";
    private const string DataKey = "leaderboard:data";

    private readonly IDatabase _db;

    public RedisLeaderboardStore(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task StoreAsync(
        IReadOnlyCollection<RankedDistrict> rankedDistricts,
        CancellationToken cancellationToken
    )
    {
        if (rankedDistricts.Count == 0)
            return;

        var batch = _db.CreateBatch();
        var tasks = new List<Task>();

        tasks.Add(batch.KeyDeleteAsync(RankKey));
        tasks.Add(batch.KeyDeleteAsync(DataKey));

        foreach (var district in rankedDistricts)
        {
            tasks.Add(
                batch.SortedSetAddAsync(RankKey, district.DistrictId.ToString(), district.Rank)
            );

            tasks.Add(
                batch.HashSetAsync(
                    DataKey,
                    district.DistrictId.ToString(),
                    JsonSerializer.Serialize(district)
                )
            );
        }

        batch.Execute();
        await Task.WhenAll(tasks);
    }

    public async Task<IReadOnlyCollection<RankedDistrict>> GetTopDistrictsAsync(
        int count,
        CancellationToken cancellationToken
    )
    {
        var ids = await _db.SortedSetRangeByRankAsync(RankKey, 0, count - 1, Order.Ascending);

        if (ids.Length == 0)
            return Array.Empty<RankedDistrict>();

        var values = await _db.HashGetAsync(DataKey, ids);

        return values
            .Where(v => v.HasValue)
            .Select(v =>
            {
                var json = v.ToString();
                return JsonSerializer.Deserialize<RankedDistrict>(json)!;
            })
            .ToList();
    }
}
