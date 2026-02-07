namespace AppCore.Models;

public sealed record TravelRecommendationRequest(
    double Latitude,
    double Longitude,
    int DestinationDistrictId,
    DateOnly TravelDate
);