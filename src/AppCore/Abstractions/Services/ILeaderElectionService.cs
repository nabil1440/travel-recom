namespace AppCore.Abstractions.Services;

public interface ILeaderElectionService
{
    Task<bool> TryAcquireAsync(
        string lockName,
        TimeSpan ttl,
        CancellationToken cancellationToken);
}
