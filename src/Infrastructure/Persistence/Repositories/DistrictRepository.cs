namespace Infrastructure.Persistence.Repositories;

using System.Text.Json;
using AppCore.Abstractions.Persistence;
using AppCore.Models;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

public sealed class DistrictRepository : IDistrictRepository
{
    private const string CacheKey = "districts:all";

    private readonly AppDbContext _db;
    private readonly IDatabase _redis;

    public DistrictRepository(AppDbContext db, IConnectionMultiplexer redis)
    {
        _db = db;
        _redis = redis.GetDatabase();
    }

    public async Task<IReadOnlyCollection<District>> GetAllAsync(
        CancellationToken cancellationToken
    )
    {
        // Try cache
        var cached = await _redis.StringGetAsync(CacheKey);
        if (cached.HasValue)
        {
            return JsonSerializer.Deserialize<List<District>>((string)cached!)
                ?? new List<District>();
        }

        // Read DB
        var districts = await _db
            .Districts.AsNoTracking()
            .Select(d => new District(d.Id, d.Name, d.Latitude, d.Longitude))
            .ToListAsync(cancellationToken);

        // Backfill cache
        if (districts.Count > 0)
        {
            var json = JsonSerializer.Serialize(districts);
            await _redis.StringSetAsync(CacheKey, json);
        }

        return districts;
    }
}
