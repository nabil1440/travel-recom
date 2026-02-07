namespace Infrastructure.Jobs;

public interface IWeatherDataFetchJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}