namespace AppCore.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCore.Abstractions.Leaderboard;
using AppCore.Abstractions.Persistence;
using AppCore.Abstractions.Services;
using AppCore.Events;
using AppCore.Models;
using AppCore.Services;
using Microsoft.Extensions.Logging;
using Moq;

public sealed class DistrictRankingServiceTests
{
    [Fact]
    public async Task HandleWeatherBatchAsync_RanksAndStores_WhenLeader()
    {
        var districts = new List<District>
        {
            new(1, "Alpha", 10, 10),
            new(2, "Beta", 20, 20)
        };

        var @event = new WeatherDataBatchFetched(
            DateTime.UtcNow,
            new List<DistrictWeatherFacts>
            {
                new(1, new List<DailyWeatherFact>
                {
                    new(new DateOnly(2026, 2, 7), 11, 6),
                    new(new DateOnly(2026, 2, 8), 11, 6)
                }),
                new(2, new List<DailyWeatherFact>
                {
                    new(new DateOnly(2026, 2, 7), 9, 8),
                    new(new DateOnly(2026, 2, 8), 9, 8)
                })
            },
            "batch-1");

        var snapshotRepository = new Mock<IWeatherSnapshotRepository>();
        var leaderboardSnapshotRepository = new Mock<ILeaderboardSnapshotRepository>();
        var leaderboardStore = new Mock<ILeaderboardStore>();
        var districtService = new Mock<IDistrictService>();
        var leaderElection = new Mock<ILeaderElectionService>();
        var logger = new Mock<ILogger<DistrictRankingService>>();

        leaderElection
            .Setup(l => l.TryAcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        districtService
            .Setup(d => d.GetDistrictsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(districts);

        IReadOnlyCollection<RankedDistrict>? storedRankings = null;
        leaderboardStore
            .Setup(s => s.StoreAsync(It.IsAny<IReadOnlyCollection<RankedDistrict>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<RankedDistrict>, CancellationToken>((rankings, _) => storedRankings = rankings)
            .Returns(Task.CompletedTask);

        var service = new DistrictRankingService(
            snapshotRepository.Object,
            leaderboardSnapshotRepository.Object,
            districtService.Object,
            leaderElection.Object,
            leaderboardStore.Object,
            logger.Object);

        await service.HandleWeatherBatchAsync(@event, CancellationToken.None);

        snapshotRepository.Verify(
            r => r.SaveAsync(It.IsAny<IReadOnlyCollection<DistrictWeatherSnapshot>>(), It.IsAny<CancellationToken>()),
            Times.Once);

        leaderboardSnapshotRepository.Verify(
            r => r.SaveAsync(It.IsAny<IReadOnlyCollection<RankedDistrict>>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.NotNull(storedRankings);
        var ordered = storedRankings!.ToList();
        Assert.Equal(2, ordered.Count);
        Assert.Equal(2, ordered[0].DistrictId);
        Assert.Equal(1, ordered[0].Rank);
        Assert.Equal(1, ordered[1].DistrictId);
        Assert.Equal(2, ordered[1].Rank);
    }

    [Fact]
    public async Task HandleWeatherBatchAsync_Skips_WhenNotLeader()
    {
        var @event = new WeatherDataBatchFetched(
            DateTime.UtcNow,
            new List<DistrictWeatherFacts>(),
            "batch-2");

        var snapshotRepository = new Mock<IWeatherSnapshotRepository>();
        var leaderboardSnapshotRepository = new Mock<ILeaderboardSnapshotRepository>();
        var leaderboardStore = new Mock<ILeaderboardStore>();
        var districtService = new Mock<IDistrictService>();
        var leaderElection = new Mock<ILeaderElectionService>();
        var logger = new Mock<ILogger<DistrictRankingService>>();

        leaderElection
            .Setup(l => l.TryAcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new DistrictRankingService(
            snapshotRepository.Object,
            leaderboardSnapshotRepository.Object,
            districtService.Object,
            leaderElection.Object,
            leaderboardStore.Object,
            logger.Object);

        await service.HandleWeatherBatchAsync(@event, CancellationToken.None);

        snapshotRepository.Verify(
            r => r.SaveAsync(It.IsAny<IReadOnlyCollection<DistrictWeatherSnapshot>>(), It.IsAny<CancellationToken>()),
            Times.Never);

        leaderboardStore.Verify(
            s => s.StoreAsync(It.IsAny<IReadOnlyCollection<RankedDistrict>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
