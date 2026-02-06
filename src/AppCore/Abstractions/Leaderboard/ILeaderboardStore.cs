namespace AppCore.Abstractions.Leaderboard;

using AppCore.Models;

public interface ILeaderboardStore
{
    Task StoreAsync(
        IReadOnlyCollection<RankedDistrict> rankedDistricts,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RankedDistrict>> GetTopDistrictsAsync(int count, CancellationToken cancellationToken);
}