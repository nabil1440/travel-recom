namespace AppCore.Abstractions.Services;

using AppCore.Models;

public interface ITravelComparisonService
{
    TravelRecommendationResult Compare(
        DailyDistrictForecast source,
        DailyDistrictForecast destination);
}