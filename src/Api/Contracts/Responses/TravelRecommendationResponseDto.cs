namespace Api.Contracts.Responses;

public sealed record TravelRecommendationResponseDto(
    string Recommendation,
    string Reason
);