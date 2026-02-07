namespace Infrastructure.Persistence.Repositories;

using AppCore.Abstractions.Persistence;
using AppCore.Models;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class DailyDistrictForecastRepository : IDailyDistrictForecastRepository
{
    private readonly AppDbContext _db;

    public DailyDistrictForecastRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DailyDistrictForecast?> GetAsync(
        int districtId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var entity = await _db.Set<DailyDistrictForecastEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.DistrictId == districtId &&
                     x.ForecastDate == date,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return new DailyDistrictForecast(
            entity.DistrictId,
            entity.ForecastDate,
            entity.Temp2Pm,
            entity.Pm25_2Pm);
    }

    public async Task SaveAsync(
        IEnumerable<DailyDistrictForecast> forecasts,
        CancellationToken cancellationToken)
    {
        foreach (var forecast in forecasts)
        {
            // Upsert semantics: one row per (DistrictId, ForecastDate)
            var entity = await _db.Set<DailyDistrictForecastEntity>()
                .FirstOrDefaultAsync(
                    x => x.DistrictId == forecast.DistrictId &&
                         x.ForecastDate == forecast.ForecastDate,
                    cancellationToken);

            if (entity is null)
            {
                entity = new DailyDistrictForecastEntity
                {
                    DistrictId = forecast.DistrictId,
                    ForecastDate = forecast.ForecastDate,
                    Temp2Pm = forecast.Temp2Pm,
                    Pm25_2Pm = forecast.Pm25_2Pm,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Add(entity);
            }
            else
            {
                entity.Temp2Pm = forecast.Temp2Pm;
                entity.Pm25_2Pm = forecast.Pm25_2Pm;
                entity.CreatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}