namespace Api.Contracts.Responses;

public sealed record LeaderboardDistrictResponse(
    int DistrictId,
    string DistrictName,
    double Temp2Pm,
    double Pm25_2Pm,
    int Rank
);
