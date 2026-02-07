namespace AppCore.Models;

public sealed record TravelRecommendationRequest(
    double Latitude,
    double Longitude,
    string DestinationDistrictName,
    DateOnly TravelDate
);