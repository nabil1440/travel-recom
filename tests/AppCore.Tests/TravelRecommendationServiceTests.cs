namespace AppCore.Tests;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AppCore.Abstractions.Services;
using AppCore.Models;
using AppCore.Services;
using Moq;

public sealed class TravelRecommendationServiceTests
{
    [Fact]
    public async Task RecommendAsync_ReturnsRecommendation_WhenDataAvailable()
    {
        var sourceDistrict = new District(1, "Source", 10, 10);
        var destinationDistrict = new District(2, "Destination", 20, 20);

        var sourceForecast = new DailyDistrictForecast(1, new DateOnly(2026, 2, 7), 30, 60);
        var destinationForecast = new DailyDistrictForecast(2, new DateOnly(2026, 2, 7), 24, 40);

        var resolver = new Mock<ISourceDistrictResolver>();
        resolver.Setup(r => r.ResolveAsync(10, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SourceDistrictResolutionResult.Success(sourceDistrict.Id));

        var lookup = new Mock<IForecastLookupService>();
        lookup.Setup(l => l.GetForecastAsync(sourceDistrict.Id, new DateOnly(2026, 2, 7), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ForecastLookupResult.Success(sourceForecast));
        lookup.Setup(l => l.GetForecastAsync(destinationDistrict.Id, new DateOnly(2026, 2, 7), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ForecastLookupResult.Success(destinationForecast));

        var districts = new Mock<IDistrictService>();
        districts.Setup(d => d.GetDistrictsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<District> { sourceDistrict, destinationDistrict });

        var comparison = new TravelComparisonService();
        var service = new TravelRecommendationService(
            resolver.Object,
            lookup.Object,
            comparison,
            districts.Object);

        var result = await service.RecommendAsync(
            new TravelRecommendationRequest(10, 10, destinationDistrict.Name, new DateOnly(2026, 2, 7)),
            CancellationToken.None);

        Assert.True(result.IsRecommended);
        Assert.Equal(RecommendationReasonCode.DestinationCoolerAndCleaner, result.ReasonCode);
    }

    [Fact]
    public async Task RecommendAsync_ReturnsInvalidSource_WhenResolutionFails()
    {
        var resolver = new Mock<ISourceDistrictResolver>();
        resolver.Setup(r => r.ResolveAsync(10, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SourceDistrictResolutionResult.NotFound());

        var lookup = new Mock<IForecastLookupService>();
        var districts = new Mock<IDistrictService>();
        var comparison = new Mock<ITravelComparisonService>();

        var service = new TravelRecommendationService(
            resolver.Object,
            lookup.Object,
            comparison.Object,
            districts.Object);

        var result = await service.RecommendAsync(
            new TravelRecommendationRequest(10, 10, "Destination", new DateOnly(2026, 2, 7)),
            CancellationToken.None);

        Assert.False(result.IsRecommended);
        Assert.Equal(RecommendationReasonCode.InvalidSourceDistrict, result.ReasonCode);
    }

    [Fact]
    public async Task RecommendAsync_ReturnsInsufficientData_WhenForecastMissing()
    {
        var sourceDistrict = new District(1, "Source", 10, 10);
        var destinationDistrict = new District(2, "Destination", 20, 20);
        var resolver = new Mock<ISourceDistrictResolver>();
        resolver.Setup(r => r.ResolveAsync(10, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SourceDistrictResolutionResult.Success(sourceDistrict.Id));

        var lookup = new Mock<IForecastLookupService>();
        lookup.Setup(l => l.GetForecastAsync(sourceDistrict.Id, new DateOnly(2026, 2, 7), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ForecastLookupResult.Success(new DailyDistrictForecast(1, new DateOnly(2026, 2, 7), 30, 60)));
        lookup.Setup(l => l.GetForecastAsync(destinationDistrict.Id, new DateOnly(2026, 2, 7), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ForecastLookupResult.NotFound());

        var districts = new Mock<IDistrictService>();
        districts.Setup(d => d.GetDistrictsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<District> { sourceDistrict, destinationDistrict });

        var comparison = new Mock<ITravelComparisonService>();
        var service = new TravelRecommendationService(
            resolver.Object,
            lookup.Object,
            comparison.Object,
            districts.Object);

        var result = await service.RecommendAsync(
            new TravelRecommendationRequest(10, 10, destinationDistrict.Name, new DateOnly(2026, 2, 7)),
            CancellationToken.None);

        Assert.False(result.IsRecommended);
        Assert.Equal(RecommendationReasonCode.InsufficientData, result.ReasonCode);
    }
}
