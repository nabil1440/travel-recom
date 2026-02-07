namespace AppCore.Abstractions.Services;

public interface IWeatherDataBatchFetchService
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
