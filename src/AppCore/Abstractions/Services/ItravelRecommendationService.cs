namespace AppCore.Abstractions.Services;

using AppCore.Models;

public interface ITravelRecommendationService
{
    Task<TravelRecommendationResult> RecommendAsync(
        TravelRecommendationRequest request,
        CancellationToken cancellationToken);
}