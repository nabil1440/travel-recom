namespace Infrastructure.Events.Consumers;

using AppCore.Abstractions.Services;
using AppCore.Events;
using Infrastructure.Events;
using Microsoft.Extensions.Logging;

public sealed class DailyAggregationConsumer : IEventConsumer<WeatherDataBatchFetched>
{
    private readonly IDailyForecastAggregationService _aggregationService;
    private readonly ILogger<DailyAggregationConsumer> _logger;

    public DailyAggregationConsumer(
        IDailyForecastAggregationService aggregationService,
        ILogger<DailyAggregationConsumer> logger)
    {
        _aggregationService = aggregationService;
        _logger = logger;
    }

    public async Task ConsumeAsync(WeatherDataBatchFetched @event)
    {
        _logger.LogInformation(
            "DailyAggregationConsumer processing batch {BatchId} with {Count} districts",
            @event.BatchId, @event.Districts.Count);

        await _aggregationService.HandleWeatherBatchAsync(@event, CancellationToken.None);
    }
}
