namespace Infrastructure.Events;

public interface IEventConsumer<TEvent>
    where TEvent : class
{
    Task ConsumeAsync(TEvent @event, CancellationToken cancellationToken);
}