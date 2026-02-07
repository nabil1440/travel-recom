namespace AppCore.Tests;

using AppCore.Models;
using AppCore.Services;

public sealed class TravelComparisonServiceTests
{
    [Fact]
    public void Compare_ReturnsRecommended_WhenDestinationCoolerAndCleaner()
    {
        var service = new TravelComparisonService();

        var source = new DailyDistrictForecast(1, new DateOnly(2026, 2, 7), 30, 60);
        var destination = new DailyDistrictForecast(2, new DateOnly(2026, 2, 7), 25, 40);

        var result = service.Compare(source, destination);

        Assert.True(result.IsRecommended);
        Assert.Equal(RecommendationReasonCode.DestinationCoolerAndCleaner, result.ReasonCode);
        Assert.Equal(-5, result.TempDelta, 3);
        Assert.Equal(-20, result.AirQualityDelta, 3);
    }

    [Fact]
    public void Compare_ReturnsHotterAndMorePolluted_WhenDestinationIsWorse()
    {
        var service = new TravelComparisonService();

        var source = new DailyDistrictForecast(1, new DateOnly(2026, 2, 7), 25, 40);
        var destination = new DailyDistrictForecast(2, new DateOnly(2026, 2, 7), 30, 60);

        var result = service.Compare(source, destination);

        Assert.False(result.IsRecommended);
        Assert.Equal(RecommendationReasonCode.DestinationHotterAndMorePolluted, result.ReasonCode);
        Assert.Equal(5, result.TempDelta, 3);
        Assert.Equal(20, result.AirQualityDelta, 3);
    }

    [Fact]
    public void Compare_ReturnsHotter_WhenDestinationIsHotterButCleaner()
    {
        var service = new TravelComparisonService();

        var source = new DailyDistrictForecast(1, new DateOnly(2026, 2, 7), 25, 60);
        var destination = new DailyDistrictForecast(2, new DateOnly(2026, 2, 7), 28, 40);

        var result = service.Compare(source, destination);

        Assert.False(result.IsRecommended);
        Assert.Equal(RecommendationReasonCode.DestinationHotter, result.ReasonCode);
        Assert.Equal(3, result.TempDelta, 3);
        Assert.Equal(-20, result.AirQualityDelta, 3);
    }

    [Fact]
    public void Compare_ReturnsMorePolluted_WhenDestinationIsCoolerButDirtier()
    {
        var service = new TravelComparisonService();

        var source = new DailyDistrictForecast(1, new DateOnly(2026, 2, 7), 30, 40);
        var destination = new DailyDistrictForecast(2, new DateOnly(2026, 2, 7), 25, 55);

        var result = service.Compare(source, destination);

        Assert.False(result.IsRecommended);
        Assert.Equal(RecommendationReasonCode.DestinationMorePolluted, result.ReasonCode);
        Assert.Equal(-5, result.TempDelta, 3);
        Assert.Equal(15, result.AirQualityDelta, 3);
    }
}
