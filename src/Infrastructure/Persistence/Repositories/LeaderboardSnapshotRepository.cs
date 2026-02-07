namespace Infrastructure.Persistence.Repositories;

using System.Text.Json;
using AppCore.Abstractions.Persistence;
using AppCore.Models;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;

public sealed class LeaderboardSnapshotRepository : ILeaderboardSnapshotRepository
{
    private readonly AppDbContext _dbContext;

    public LeaderboardSnapshotRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveAsync(
        IReadOnlyCollection<RankedDistrict> rankedDistricts,
        DateTime generatedAtUtc,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(rankedDistricts);

        _dbContext.LeaderboardSnapshots.Add(
            new LeaderboardSnapshotEntity
            {
                GeneratedAt = generatedAtUtc,
                PayloadJson = payload
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
