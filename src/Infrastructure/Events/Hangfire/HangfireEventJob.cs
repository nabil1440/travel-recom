namespace Infrastructure.Events.Hangfire;

using Infrastructure.Events;
using Microsoft.Extensions.Logging;

public sealed class HangfireEventJob<TEvent>
    where TEvent : class
{
    private readonly IEnumerable<IEventConsumer<TEvent>> _consumers;
    private readonly ILogger<HangfireEventJob<TEvent>> _logger;

    public HangfireEventJob(
        IEnumerable<IEventConsumer<TEvent>> consumers,
        ILogger<HangfireEventJob<TEvent>> logger)
    {
        _consumers = consumers;
        _logger = logger;
    }

    public async Task ExecuteAsync(TEvent @event)
    {
        foreach (var consumer in _consumers)
        {
            await RunConsumerAsync(consumer, @event);
        }
    }

    private async Task RunConsumerAsync(IEventConsumer<TEvent> consumer, TEvent @event)
    {
        try
        {
            await consumer.ConsumeAsync(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Consumer {ConsumerType} failed while handling {EventType}. Skipping (no retry).",
                consumer.GetType().Name,
                typeof(TEvent).Name);
        }
    }
}