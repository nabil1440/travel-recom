 namespace Infrastructure.LeaderElection;

using StackExchange.Redis;

public sealed class RedisLeaderElectionService
{
    private readonly IDatabase _db;
    private const string LeaderKey = "leaderboard:leader";

    public RedisLeaderElectionService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<bool> TryAcquireAsync(
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        return await _db.StringSetAsync(
            LeaderKey,
            Environment.MachineName,
            ttl,
            when: When.NotExists);
    }
}