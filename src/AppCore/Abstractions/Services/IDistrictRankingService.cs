namespace AppCore.Abstractions.Services;

using AppCore.Models;

public interface IDistrictRankingService
{
    Task RefreshLeaderboardAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RankedDistrict>> GetTopDistrictsAsync(
        int count,
        CancellationToken cancellationToken
    );
}
