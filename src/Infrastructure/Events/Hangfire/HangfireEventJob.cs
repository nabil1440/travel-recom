namespace Infrastructure.Events.Hangfire;

using Infrastructure.Events;

public sealed class HangfireEventJob<TEvent>
    where TEvent : class
{
    private readonly IEnumerable<IEventConsumer<TEvent>> _consumers;

    public HangfireEventJob(IEnumerable<IEventConsumer<TEvent>> consumers)
    {
        _consumers = consumers;
    }

    public async Task ExecuteAsync(TEvent @event)
    {
        foreach (var consumer in _consumers)
        {
            await consumer.ConsumeAsync(@event);
        }
    }
}