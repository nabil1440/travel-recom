namespace AppCore.Services;

using AppCore.Abstractions.Services;
using AppCore.Models;

public sealed class TravelComparisonService : ITravelComparisonService
{
    public TravelRecommendationResult Compare(
        DailyDistrictForecast source,
        DailyDistrictForecast destination)
    {
        var tempDelta = destination.Temp2Pm - source.Temp2Pm;
        var airDelta = destination.Pm25_2Pm - source.Pm25_2Pm;

        if (tempDelta < 0 && airDelta < 0)
        {
            return new TravelRecommendationResult(
                IsRecommended: true,
                TempDelta: tempDelta,
                AirQualityDelta: airDelta,
                ReasonCode: RecommendationReasonCode.DestinationCoolerAndCleaner);
        }

        if (tempDelta >= 0 && airDelta >= 0)
        {
            return new TravelRecommendationResult(
                IsRecommended: false,
                TempDelta: tempDelta,
                AirQualityDelta: airDelta,
                ReasonCode: RecommendationReasonCode.DestinationHotterAndMorePolluted);
        }

        if (tempDelta >= 0)
        {
            return new TravelRecommendationResult(
                IsRecommended: false,
                TempDelta: tempDelta,
                AirQualityDelta: airDelta,
                ReasonCode: RecommendationReasonCode.DestinationHotter);
        }

        // temp is better, air is worse
        return new TravelRecommendationResult(
            IsRecommended: false,
            TempDelta: tempDelta,
            AirQualityDelta: airDelta,
            ReasonCode: RecommendationReasonCode.DestinationMorePolluted);
    }
}