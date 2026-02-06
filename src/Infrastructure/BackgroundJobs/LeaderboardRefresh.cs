namespace Infrastructure.BackgroundJobs;

using AppCore.Abstractions.Services;
using Infrastructure.LeaderElection;
using Microsoft.Extensions.Logging;

public sealed class LeaderboardRefreshJob
{
    private readonly IDistrictRankingService _rankingService;
    private readonly RedisLeaderElectionService _leaderElection;
    private readonly ILogger<LeaderboardRefreshJob> _logger;

    public LeaderboardRefreshJob(
        IDistrictRankingService rankingService,
        RedisLeaderElectionService leaderElection,
        ILogger<LeaderboardRefreshJob> logger)
    {
        _rankingService = rankingService;
        _leaderElection = leaderElection;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var acquired = await _leaderElection.TryAcquireAsync(
            TimeSpan.FromMinutes(10),
            cancellationToken);

        if (!acquired)
        {
            _logger.LogInformation("Leaderboard refresh skipped: not leader");
            return;
        }

        _logger.LogInformation("Leaderboard refresh started");

        await _rankingService.RefreshLeaderboardAsync(cancellationToken);

        _logger.LogInformation("Leaderboard refresh completed");
    }
}