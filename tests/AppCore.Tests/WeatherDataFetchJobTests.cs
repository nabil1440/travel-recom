using AppCore.Abstractions.Aggregation;
using AppCore.Abstractions.DataFetching;
using AppCore.Abstractions.Events;
using AppCore.Abstractions.Persistence;
using AppCore.Events;
using AppCore.Models;
using Infrastructure.BackgroundJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AppCore.Tests;

public class WeatherDataFetchJobTests
{
    [Fact]
    public async Task RunAsync_WithValidDistricts_FetchesAndPublishesEvents()
    {
        // Arrange
        var mockDataFetchingService = new Mock<IDataFetchingService>();
        var mockAggregationService = new Mock<IWeatherAggregationService>();
        var mockDistrictRepository = new Mock<IDistrictRepository>();
        var mockEventPublisher = new Mock<IEventPublisher>();
        var mockLogger = new Mock<ILogger<WeatherDataFetchJob>>();

        var testDistrict = new District(1, "Dhaka", 23.8103, 90.4125);
        var districts = new List<District> { testDistrict };

        var weatherForecast = new RawWeatherForecast(
            new List<DateTime> { DateTime.UtcNow.Date.AddHours(8) },
            new List<double> { 25.5 });

        var airQualityForecast = new RawAirQualityForecast(
            new List<DateTime> { DateTime.UtcNow.Date.AddHours(8) },
            new List<double> { 50.0 });

        var snapshot = new DistrictWeatherSnapshot(
            testDistrict.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            25.5,
            50.0);

        mockDistrictRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(districts);

        mockDataFetchingService
            .Setup(x => x.GetWeatherForecastAsync(
                testDistrict.Latitude,
                testDistrict.Longitude,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(weatherForecast);

        mockDataFetchingService
            .Setup(x => x.GetAirQualityForecastAsync(
                testDistrict.Latitude,
                testDistrict.Longitude,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(airQualityForecast);

        mockAggregationService
            .Setup(x => x.Aggregate(testDistrict, weatherForecast, airQualityForecast))
            .Returns(snapshot);

        var job = new WeatherDataFetchJob(
            mockDataFetchingService.Object,
            mockAggregationService.Object,
            mockDistrictRepository.Object,
            mockEventPublisher.Object,
            mockLogger.Object);

        // Act
        await job.RunAsync(CancellationToken.None);

        // Assert
        mockDistrictRepository.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        mockDataFetchingService.Verify(
            x => x.GetWeatherForecastAsync(
                testDistrict.Latitude,
                testDistrict.Longitude,
                It.IsAny<CancellationToken>()),
            Times.Once);

        mockDataFetchingService.Verify(
            x => x.GetAirQualityForecastAsync(
                testDistrict.Latitude,
                testDistrict.Longitude,
                It.IsAny<CancellationToken>()),
            Times.Once);

        mockAggregationService.Verify(
            x => x.Aggregate(testDistrict, weatherForecast, airQualityForecast),
            Times.Once);

        mockEventPublisher.Verify(
            x => x.PublishAsync(
                It.Is<WeatherDataFetchedEvent>(e =>
                    e.DistrictId == testDistrict.Id &&
                    e.DistrictName == testDistrict.Name &&
                    e.Temperature2Pm == 25.5 &&
                    e.Pm25_2Pm == 50.0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithNoDistricts_DoesNotPublishEvents()
    {
        // Arrange
        var mockDataFetchingService = new Mock<IDataFetchingService>();
        var mockAggregationService = new Mock<IWeatherAggregationService>();
        var mockDistrictRepository = new Mock<IDistrictRepository>();
        var mockEventPublisher = new Mock<IEventPublisher>();
        var mockLogger = new Mock<ILogger<WeatherDataFetchJob>>();

        mockDistrictRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<District>());

        var job = new WeatherDataFetchJob(
            mockDataFetchingService.Object,
            mockAggregationService.Object,
            mockDistrictRepository.Object,
            mockEventPublisher.Object,
            mockLogger.Object);

        // Act
        await job.RunAsync(CancellationToken.None);

        // Assert
        mockEventPublisher.Verify(
            x => x.PublishAsync(
                It.IsAny<WeatherDataFetchedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RunAsync_WhenFetchingFails_ContinuesWithOtherDistricts()
    {
        // Arrange
        var mockDataFetchingService = new Mock<IDataFetchingService>();
        var mockAggregationService = new Mock<IWeatherAggregationService>();
        var mockDistrictRepository = new Mock<IDistrictRepository>();
        var mockEventPublisher = new Mock<IEventPublisher>();
        var mockLogger = new Mock<ILogger<WeatherDataFetchJob>>();

        var testDistrict1 = new District(1, "Dhaka", 23.8103, 90.4125);
        var testDistrict2 = new District(2, "Chittagong", 22.3569, 91.7832);
        var districts = new List<District> { testDistrict1, testDistrict2 };

        mockDistrictRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(districts);

        // First district fails
        mockDataFetchingService
            .Setup(x => x.GetWeatherForecastAsync(
                testDistrict1.Latitude,
                testDistrict1.Longitude,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API Error"));

        // Second district succeeds
        var weatherForecast = new RawWeatherForecast(
            new List<DateTime> { DateTime.UtcNow.Date.AddHours(8) },
            new List<double> { 26.0 });

        var airQualityForecast = new RawAirQualityForecast(
            new List<DateTime> { DateTime.UtcNow.Date.AddHours(8) },
            new List<double> { 55.0 });

        var snapshot = new DistrictWeatherSnapshot(
            testDistrict2.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            26.0,
            55.0);

        mockDataFetchingService
            .Setup(x => x.GetWeatherForecastAsync(
                testDistrict2.Latitude,
                testDistrict2.Longitude,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(weatherForecast);

        mockDataFetchingService
            .Setup(x => x.GetAirQualityForecastAsync(
                testDistrict2.Latitude,
                testDistrict2.Longitude,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(airQualityForecast);

        mockAggregationService
            .Setup(x => x.Aggregate(testDistrict2, weatherForecast, airQualityForecast))
            .Returns(snapshot);

        var job = new WeatherDataFetchJob(
            mockDataFetchingService.Object,
            mockAggregationService.Object,
            mockDistrictRepository.Object,
            mockEventPublisher.Object,
            mockLogger.Object);

        // Act
        await job.RunAsync(CancellationToken.None);

        // Assert - event should be published for the successful district
        mockEventPublisher.Verify(
            x => x.PublishAsync(
                It.Is<WeatherDataFetchedEvent>(e => e.DistrictId == testDistrict2.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
