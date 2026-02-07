namespace Infrastructure.Redis;

using Infrastructure.Redis.Models;

public interface IDailyForecastCache
{
    Task<DailyForecastCacheItem?> GetAsync(
        int districtId,
        DateOnly forecastDate,
        CancellationToken cancellationToken);

    Task SetAsync(
        DailyForecastCacheItem item,
        TimeSpan ttl,
        CancellationToken cancellationToken);
}