namespace Infrastructure.Services;

using AppCore.Abstractions.Persistence;
using AppCore.Abstractions.Services;
using AppCore.Models;

public sealed class ForecastLookupService : IForecastLookupService
{
    private readonly IDailyForecastCache _cache;
    private readonly IDailyDistrictForecastRepository _repository;
    private readonly IDistrictService _districtService;

    // TTL slightly longer than forecast horizon
    private static readonly TimeSpan CacheTtl = TimeSpan.FromDays(8);

    public ForecastLookupService(
        IDailyForecastCache cache,
        IDailyDistrictForecastRepository repository,
        IDistrictService districtService)
    {
        _cache = cache;
        _repository = repository;
        _districtService = districtService;
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
        var forecast = await _repository.GetAsync(districtId, date, cancellationToken);

        if (forecast is null)
        {
            return ForecastLookupResult.NotFound();
        }

        // 3. Hydrate cache (best-effort)
        try
        {
            var districts = await _districtService.GetDistrictsAsync(cancellationToken);
            var districtMap = districts.ToDictionary(d => d.Id);

            if (districtMap.TryGetValue(forecast.DistrictId, out var district))
            {
                await _cache.SetAsync(
                    new DailyForecastCacheItem(
                        forecast.DistrictId,
                        district.Name,
                        forecast.ForecastDate,
                        forecast.Temp2Pm,
                        forecast.Pm25_2Pm),
                    CacheTtl,
                    cancellationToken);
            }
        }
        catch
        {
            // Again: Redis failure is not fatal
        }

        return ForecastLookupResult.Success(forecast);
    }
}