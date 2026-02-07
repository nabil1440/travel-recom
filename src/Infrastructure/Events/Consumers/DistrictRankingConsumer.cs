namespace Infrastructure.Events.Consumers;

using AppCore.Abstractions.Services;
using AppCore.Events;
using Infrastructure.Events;
using Microsoft.Extensions.Logging;

public sealed class DistrictRankingConsumer : IEventConsumer<WeatherDataBatchFetched>
{
    private readonly IDistrictRankingService _rankingService;
    private readonly ILogger<DistrictRankingConsumer> _logger;

    public DistrictRankingConsumer(
        IDistrictRankingService rankingService,
        ILogger<DistrictRankingConsumer> logger)
    {
        _rankingService = rankingService;
        _logger = logger;
    }

    public async Task ConsumeAsync(WeatherDataBatchFetched @event)
    {
        _logger.LogInformation(
            "DistrictRankingConsumer processing batch {BatchId} with {Count} districts",
            @event.BatchId, @event.Districts.Count);

        await _rankingService.HandleWeatherBatchAsync(@event, CancellationToken.None);
    }
}
