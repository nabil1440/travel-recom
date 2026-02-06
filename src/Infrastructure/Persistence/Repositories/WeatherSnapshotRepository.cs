namespace Infrastructure.Persistence.Repositories;

using AppCore.Abstractions.Persistence;
using AppCore.Models;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class WeatherSnapshotRepository : IWeatherSnapshotRepository
{
    private readonly AppDbContext _dbContext;

    public WeatherSnapshotRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveAsync(
        IReadOnlyCollection<DistrictWeatherSnapshot> snapshots,
        CancellationToken cancellationToken
    )
    {
        if (snapshots.Count == 0)
            return;

        foreach (var snapshot in snapshots)
        {
            var entity = await _dbContext.DistrictWeatherSnapshots.FirstOrDefaultAsync(
                e => e.DistrictId == snapshot.DistrictId && e.Date == snapshot.Date,
                cancellationToken
            );

            if (entity is null)
            {
                _dbContext.DistrictWeatherSnapshots.Add(
                    new DistrictWeatherSnapshotEntity
                    {
                        DistrictId = snapshot.DistrictId,
                        Date = snapshot.Date,
                        Temp2Pm = snapshot.Temp2Pm,
                        Pm25_2Pm = snapshot.Pm25_2Pm,
                        CreatedAt = DateTime.UtcNow
                    }
                );
            }
            else
            {
                entity.Temp2Pm = snapshot.Temp2Pm;
                entity.Pm25_2Pm = snapshot.Pm25_2Pm;
                entity.CreatedAt = DateTime.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<DistrictWeatherSnapshot>> GetLatestAsync(
        CancellationToken cancellationToken
    )
    {
        var latestDate = await _dbContext.DistrictWeatherSnapshots.MaxAsync(
            e => (DateOnly?)e.Date,
            cancellationToken
        );

        if (latestDate is null)
            return Array.Empty<DistrictWeatherSnapshot>();

        var entities = await _dbContext
            .DistrictWeatherSnapshots.Where(e => e.Date == latestDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return entities
            .Select(e => new DistrictWeatherSnapshot(e.DistrictId, e.Date, e.Temp2Pm, e.Pm25_2Pm))
            .ToList();
    }
}
