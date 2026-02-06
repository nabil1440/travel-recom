namespace Infrastructure;

using AppCore.Abstractions.Aggregation;
using AppCore.Abstractions.DataFetching;
using AppCore.Abstractions.Leaderboard;
using AppCore.Abstractions.Persistence;
using AppCore.Abstractions.Services;
using AppCore.Services;
using Infrastructure.BackgroundJobs;
using Infrastructure.Caching;
using Infrastructure.DataFetching.OpenMeteo;
using Infrastructure.LeaderElection;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // ---------- Database ----------
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("Postgres")));

        // ---------- Redis ----------
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(
                config.GetConnectionString("Redis")));

        // ---------- External data fetching ----------
        services.AddHttpClient<IDataFetchingService, OpenMeteoDataFetchingService>(
            client =>
            {
                client.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
                client.Timeout = TimeSpan.FromSeconds(10);
            });

        // ---------- AppCore logic ----------
        services.AddScoped<IWeatherAggregationService, WeatherAggregationService>();
        services.AddScoped<IDistrictRankingService, DistrictRankingService>();

        // ---------- Persistence ----------
        services.AddScoped<IWeatherSnapshotRepository, WeatherSnapshotRepository>();

        // ---------- Leaderboard ----------
        services.AddScoped<ILeaderboardStore, RedisLeaderboardStore>();

        // ---------- Background & infra helpers ----------
        services.AddSingleton<RedisLeaderElectionService>();
        services.AddScoped<LeaderboardRefreshJob>();

        return services;
    }
}