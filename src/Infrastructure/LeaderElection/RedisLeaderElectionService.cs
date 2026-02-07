 namespace Infrastructure.LeaderElection;

using AppCore.Abstractions.Services;
using StackExchange.Redis;

public sealed class RedisLeaderElectionService : ILeaderElectionService
{
    private readonly IDatabase _db;
    private const string LeaderKeyPrefix = "leader";

    public RedisLeaderElectionService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<bool> TryAcquireAsync(
        string lockName,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        var key = $"{LeaderKeyPrefix}:{lockName}";

        return await _db.StringSetAsync(
            key,
            Environment.MachineName,
            ttl,
            when: When.NotExists);
    }
}