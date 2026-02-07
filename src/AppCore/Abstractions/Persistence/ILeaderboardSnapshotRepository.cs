namespace AppCore.Abstractions.Persistence;

using AppCore.Models;

public interface ILeaderboardSnapshotRepository
{
    Task SaveAsync(
        IReadOnlyCollection<RankedDistrict> rankedDistricts,
        DateTime generatedAtUtc,
        CancellationToken cancellationToken);
}
