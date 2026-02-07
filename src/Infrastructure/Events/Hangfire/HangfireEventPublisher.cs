namespace Infrastructure.Events.Hangfire;

using AppCore.Abstractions.Events;
using global::Hangfire;
using Microsoft.Extensions.DependencyInjection;

public sealed class HangfireEventPublisher : IEventPublisher
{
    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly IServiceProvider _serviceProvider;

    public HangfireEventPublisher(
        IBackgroundJobClient backgroundJobs,
        IServiceProvider serviceProvider)
    {
        _backgroundJobs = backgroundJobs;
        _serviceProvider = serviceProvider;
    }

    public Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : class
    {
        var consumers = _serviceProvider
            .GetServices<IEventConsumer<TEvent>>();

        foreach (var consumer in consumers)
        {
            _backgroundJobs.Enqueue(
                () => consumer.ConsumeAsync(@event, CancellationToken.None)
            );
        }

        return Task.CompletedTask;
    }
}