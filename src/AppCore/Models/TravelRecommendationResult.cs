namespace AppCore.Models;

public sealed record TravelRecommendationResult(
    bool IsRecommended,
    double TempDelta,
    double AirQualityDelta,
    RecommendationReasonCode ReasonCode
);