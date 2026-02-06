namespace Api.Contracts.Responses;

public sealed record LeaderboardResponse(
    IReadOnlyCollection<LeaderboardDistrictResponse> Districts
);