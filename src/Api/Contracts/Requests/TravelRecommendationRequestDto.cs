namespace Api.Contracts.Requests;

public sealed record TravelRecommendationRequestDto(
    double Latitude,
    double Longitude,
    string Destination,
    DateOnly TravelDate
);