namespace Infrastructure.BackgroundJobs;

using AppCore.Abstractions.Aggregation;
using AppCore.Abstractions.BackgroundJobs;
using AppCore.Abstractions.DataFetching;
using AppCore.Abstractions.Events;
using AppCore.Abstractions.Persistence;
using AppCore.Events;
using AppCore.Models;
using Microsoft.Extensions.Logging;

public sealed class WeatherDataFetchJob : IWeatherDataFetchJob
{
    private readonly IDataFetchingService _dataFetchingService;
    private readonly IWeatherAggregationService _aggregationService;
    private readonly IDistrictRepository _districtRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<WeatherDataFetchJob> _logger;

    public WeatherDataFetchJob(
        IDataFetchingService dataFetchingService,
        IWeatherAggregationService aggregationService,
        IDistrictRepository districtRepository,
        IEventPublisher eventPublisher,
        ILogger<WeatherDataFetchJob> logger)
    {
        _dataFetchingService = dataFetchingService;
        _aggregationService = aggregationService;
        _districtRepository = districtRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Weather data fetch job started");

        var districts = await _districtRepository.GetAllAsync(cancellationToken);

        if (districts.Count == 0)
        {
            _logger.LogWarning("No districts found to fetch weather data for");
            return;
        }

        // Fetch and process weather data for all districts in parallel
        var tasks = districts.Select(district =>
            FetchAndPublishWeatherDataAsync(district, cancellationToken));

        await Task.WhenAll(tasks);

        _logger.LogInformation("Weather data fetch job completed");
    }

    private async Task FetchAndPublishWeatherDataAsync(
        District district,
        CancellationToken cancellationToken)
    {
        try
        {
            // Fetch weather and air quality data in parallel
            var weatherTask = _dataFetchingService.GetWeatherForecastAsync(
                district.Latitude,
                district.Longitude,
                cancellationToken);

            var airQualityTask = _dataFetchingService.GetAirQualityForecastAsync(
                district.Latitude,
                district.Longitude,
                cancellationToken);

            await Task.WhenAll(weatherTask, airQualityTask);

            var weather = await weatherTask;
            var airQuality = await airQualityTask;

            // Aggregate the data to extract 2PM values (GMT+6 14:00 = GMT 08:00)
            var snapshot = _aggregationService.Aggregate(
                district,
                weather,
                airQuality);

            // Publish event with the extracted 2PM data
            var weatherEvent = new WeatherDataFetchedEvent(
                snapshot.DistrictId,
                district.Name,
                snapshot.Date,
                snapshot.Temp2Pm,
                snapshot.Pm25_2Pm);

            await _eventPublisher.PublishAsync(weatherEvent, cancellationToken);

            _logger.LogInformation(
                "Published weather data for district {DistrictId} ({DistrictName})",
                district.Id,
                district.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to fetch and publish weather data for district {DistrictId} ({DistrictName})",
                district.Id,
                district.Name);
        }
    }
}
