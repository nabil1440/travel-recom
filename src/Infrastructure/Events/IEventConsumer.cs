namespace Infrastructure.Events;

public interface IEventConsumer<in TEvent>
    where TEvent : class
{
    Task ConsumeAsync(TEvent @event);
}