namespace Api.Contracts.Requests;

public sealed record TravelRecommendationRequestDto(
    double Latitude,
    double Longitude,
    int DestinationDistrictId,
    DateOnly TravelDate
);