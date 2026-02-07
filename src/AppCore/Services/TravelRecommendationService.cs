namespace AppCore.Services;

using AppCore.Abstractions.Services;
using AppCore.Models;

public sealed class TravelRecommendationService : ITravelRecommendationService
{
    private readonly ISourceDistrictResolver _sourceResolver;
    private readonly IForecastLookupService _forecastLookup;
    private readonly ITravelComparisonService _comparisonService;

    public TravelRecommendationService(
        ISourceDistrictResolver sourceResolver,
        IForecastLookupService forecastLookup,
        ITravelComparisonService comparisonService)
    {
        _sourceResolver = sourceResolver;
        _forecastLookup = forecastLookup;
        _comparisonService = comparisonService;
    }

    public async Task<TravelRecommendationResult> RecommendAsync(
        TravelRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        // 1. Resolve source district
        var sourceResult = await _sourceResolver.ResolveAsync(
            request.Latitude,
            request.Longitude,
            cancellationToken);

        if (!sourceResult.Found || sourceResult.DistrictId is null)
        {
            return new TravelRecommendationResult(
                IsRecommended: false,
                TempDelta: 0,
                AirQualityDelta: 0,
                ReasonCode: RecommendationReasonCode.InvalidSourceDistrict);
        }

        var sourceDistrictId = sourceResult.DistrictId.Value;

        // 2. Validate destination
        if (sourceDistrictId == request.DestinationDistrictId)
        {
            return new TravelRecommendationResult(
                IsRecommended: false,
                TempDelta: 0,
                AirQualityDelta: 0,
                ReasonCode: RecommendationReasonCode.SameSourceAndDestination);
        }

        // 3. Lookup forecasts (cache → DB handled internally)
        var sourceForecastResult = await _forecastLookup.GetForecastAsync(
            sourceDistrictId,
            request.TravelDate,
            cancellationToken);

        var destinationForecastResult = await _forecastLookup.GetForecastAsync(
            request.DestinationDistrictId,
            request.TravelDate,
            cancellationToken);

        if (!sourceForecastResult.Found || !destinationForecastResult.Found)
        {
            return new TravelRecommendationResult(
                IsRecommended: false,
                TempDelta: 0,
                AirQualityDelta: 0,
                ReasonCode: RecommendationReasonCode.InsufficientData);
        }

        // 4. Compare
        return _comparisonService.Compare(
            sourceForecastResult.Forecast!,
            destinationForecastResult.Forecast!);
    }
}