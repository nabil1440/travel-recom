namespace AppCore.Services;

using AppCore.Abstractions.Services;
using AppCore.Models;

public sealed class TravelRecommendationService : ITravelRecommendationService
{
    private readonly ISourceDistrictResolver _sourceResolver;
    private readonly IForecastLookupService _forecastLookup;
    private readonly ITravelComparisonService _comparisonService;
    private readonly IDistrictService _districtService;

    public TravelRecommendationService(
        ISourceDistrictResolver sourceResolver,
        IForecastLookupService forecastLookup,
        ITravelComparisonService comparisonService,
        IDistrictService districtService)
    {
        _sourceResolver = sourceResolver;
        _forecastLookup = forecastLookup;
        _comparisonService = comparisonService;
        _districtService = districtService;
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

        var destinationDistrictId = await ResolveDestinationDistrictIdAsync(
            request.DestinationDistrictName,
            cancellationToken);

        if (destinationDistrictId is null)
        {
            return new TravelRecommendationResult(
                IsRecommended: false,
                TempDelta: 0,
                AirQualityDelta: 0,
                ReasonCode: RecommendationReasonCode.InvalidDestinationDistrict);
        }

        // 2. Validate destination
        if (sourceDistrictId == destinationDistrictId.Value)
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
            destinationDistrictId.Value,
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

    private async Task<int?> ResolveDestinationDistrictIdAsync(
        string destinationDistrictName,
        CancellationToken cancellationToken)
    {
        var districts = await _districtService.GetDistrictsAsync(cancellationToken);

        var match = districts.FirstOrDefault(d =>
            string.Equals(
                d.Name,
                destinationDistrictName,
                StringComparison.OrdinalIgnoreCase));

        return match?.Id;
    }
}