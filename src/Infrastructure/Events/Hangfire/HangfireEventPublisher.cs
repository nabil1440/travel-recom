namespace Infrastructure.Events.Hangfire;

using AppCore.Abstractions.Events;
using global::Hangfire;

public sealed class HangfireEventPublisher : IEventPublisher
{
    private readonly IBackgroundJobClient _backgroundJobs;

    public HangfireEventPublisher(IBackgroundJobClient backgroundJobs)
    {
        _backgroundJobs = backgroundJobs;
    }

    public Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : class
    {
        _backgroundJobs.Enqueue<HangfireEventJob<TEvent>>(
            job => job.ExecuteAsync(@event)
        );

        return Task.CompletedTask;
    }
}