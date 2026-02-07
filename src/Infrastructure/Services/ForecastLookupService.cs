namespace Infrastructure.Services;

using AppCore.Abstractions.Services;
using AppCore.Models;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Redis;
using Infrastructure.Redis.Models;

public sealed class ForecastLookupService : IForecastLookupService
{
    private readonly IDailyForecastCache _cache;
    private readonly IDailyDistrictForecastRepository _repository;

    // TTL slightly longer than forecast horizon
    private static readonly TimeSpan CacheTtl = TimeSpan.FromDays(8);

    public ForecastLookupService(
        IDailyForecastCache cache,
        IDailyDistrictForecastRepository repository)
    {
        _cache = cache;
        _repository = repository;
    }

    public async Task<ForecastLookupResult> GetForecastAsync(
        int districtId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        // 1. Try Redis
        try
        {
            var cached = await _cache.GetAsync(districtId, date, cancellationToken);

            if (cached is not null)
            {
                return ForecastLookupResult.Success(
                    new DailyDistrictForecast(
                        cached.DistrictId,
                        cached.ForecastDate,
                        cached.Temp2Pm,
                        cached.Pm25_2Pm));
            }
        }
        catch
        {
            // Redis is optional. We do not fail the request.
        }

        // 2. Fallback to DB
        var entity = await _repository.GetAsync(districtId, date, cancellationToken);

        if (entity is null)
        {
            return ForecastLookupResult.NotFound();
        }

        var forecast = new DailyDistrictForecast(
            entity.DistrictId,
            entity.ForecastDate,
            entity.Temp2Pm,
            entity.Pm25_2Pm);

        // 3. Hydrate cache (best-effort)
        try
        {
            await _cache.SetAsync(
                new DailyForecastCacheItem(
                    entity.DistrictId,
                    entity.District.Name,
                    entity.ForecastDate,
                    entity.Temp2Pm,
                    entity.Pm25_2Pm),
                CacheTtl,
                cancellationToken);
        }
        catch
        {
            // Again: Redis failure is not fatal
        }

        return ForecastLookupResult.Success(forecast);
    }
}