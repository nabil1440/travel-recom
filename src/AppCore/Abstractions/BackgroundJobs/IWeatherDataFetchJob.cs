namespace AppCore.Abstractions.BackgroundJobs;

public interface IWeatherDataFetchJob
{
    Task RunAsync(CancellationToken cancellationToken);
}
