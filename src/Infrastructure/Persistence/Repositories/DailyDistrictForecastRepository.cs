namespace Infrastructure.Persistence.Repositories;

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

    public async Task<DailyDistrictForecastEntity?> GetAsync(
        int districtId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        return await _db.Set<DailyDistrictForecastEntity>()
            .AsNoTracking()
            .Include(x => x.District)
            .FirstOrDefaultAsync(
                x => x.DistrictId == districtId &&
                     x.ForecastDate == date,
                cancellationToken);
    }

    public async Task SaveAsync(
        IEnumerable<DailyDistrictForecastEntity> entities,
        CancellationToken cancellationToken)
    {
        foreach (var entity in entities)
        {
            // Upsert semantics: one row per (DistrictId, ForecastDate)
            var exists = await _db.Set<DailyDistrictForecastEntity>()
                .AnyAsync(
                    x => x.DistrictId == entity.DistrictId &&
                         x.ForecastDate == entity.ForecastDate,
                    cancellationToken);

            if (!exists)
            {
                entity.CreatedAt = DateTime.UtcNow;
                _db.Add(entity);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}