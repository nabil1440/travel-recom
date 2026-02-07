namespace Api.Contracts.Responses;

public sealed record TravelRecommendationResponseDto(
    bool Recommended,
    string Reason,
    double TempDelta,
    double AirQualityDelta
);