namespace AppCore.Abstractions.Services;

using AppCore.Events;

public interface IDailyForecastAggregationService
{
    Task HandleWeatherBatchAsync(
        WeatherDataBatchFetched @event,
        CancellationToken cancellationToken);
}
