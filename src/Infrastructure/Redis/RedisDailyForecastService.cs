namespace Infrastructure.Redis;

using System.Text.Json;
using AppCore.Abstractions.Persistence;
using AppCore.Models;
using StackExchange.Redis;

public sealed class RedisDailyForecastCache : IDailyForecastCache
{
    private readonly IDatabase _db;

    public RedisDailyForecastCache(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<DailyForecastCacheItem?> GetAsync(
        int districtId,
        DateOnly forecastDate,
        CancellationToken cancellationToken)
    {
        var key = BuildKey(districtId, forecastDate);

        var value = await _db.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<DailyForecastCacheItem>((string)value!);
    }

    public async Task SetAsync(
        DailyForecastCacheItem item,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        var key = BuildKey(item.DistrictId, item.ForecastDate);

        var payload = JsonSerializer.Serialize(item);

        await _db.StringSetAsync(key, payload, ttl);
    }

    private static string BuildKey(int districtId, DateOnly date)
        => $"forecast:{districtId}:{date:yyyy-MM-dd}";
}