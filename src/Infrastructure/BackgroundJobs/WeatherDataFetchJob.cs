namespace Infrastructure.BackgroundJobs;

using AppCore.Abstractions.Services;
using Infrastructure.Jobs;
using Microsoft.Extensions.Logging;

public sealed class WeatherDataFetchJob : IWeatherDataFetchJob
{
  private readonly IWeatherDataBatchFetchService _batchFetchService;
  private readonly ILogger<WeatherDataFetchJob> _logger;

  public WeatherDataFetchJob(
      IWeatherDataBatchFetchService batchFetchService,
      ILogger<WeatherDataFetchJob> logger)
  {
    _batchFetchService = batchFetchService;
    _logger = logger;
  }

  public async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    await _batchFetchService.ExecuteAsync(cancellationToken);

    _logger.LogInformation("Weather data batch fetch finished");
  }
}
