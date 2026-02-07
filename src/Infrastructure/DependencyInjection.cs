namespace Infrastructure;

using AppCore.Abstractions.Aggregation;
using AppCore.Abstractions.DataFetching;
using AppCore.Abstractions.Events;
using AppCore.Abstractions.Leaderboard;
using AppCore.Abstractions.Persistence;
using AppCore.Abstractions.Services;
using AppCore.Events;
using AppCore.Services;
using Infrastructure.BackgroundJobs;
using Infrastructure.Caching;
using Infrastructure.DataFetching.OpenMeteo;
using Infrastructure.Events;
using Infrastructure.Events.Hangfire;
using Infrastructure.Jobs;
using Infrastructure.LeaderElection;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Redis;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      IConfiguration config
  )
  {
    // ---------- Database ----------
    services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(config.GetConnectionString("Postgres"))
    );

    // ---------- Redis ----------
    services.AddSingleton<IConnectionMultiplexer>(_ =>
        ConnectionMultiplexer.Connect(config.GetConnectionString("Redis"))
    );

    // ---------- External data fetching ----------
    services.AddHttpClient(
        "OpenMeteoWeather",
        client =>
        {
          client.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
          client.Timeout = TimeSpan.FromSeconds(10);
        }
    );

    services.AddHttpClient(
        "OpenMeteoAirQuality",
        client =>
        {
          client.BaseAddress = new Uri("https://air-quality-api.open-meteo.com/v1/");
          client.Timeout = TimeSpan.FromSeconds(10);
        }
    );

    services.AddScoped<IDataFetchingService, OpenMeteoDataFetchingService>();

    // ---------- AppCore logic ----------
    services.AddScoped<IWeatherAggregationService, WeatherAggregationService>();
    services.AddScoped<IDistrictRankingService, DistrictRankingService>();
    services.AddScoped<IDistrictService, DistrictService>();

    // ---------- AppCore travel recommendation ----------
    services.AddScoped<ITravelComparisonService, TravelComparisonService>();
    services.AddScoped<ITravelRecommendationService, TravelRecommendationService>();

    // ---------- Persistence ----------
    services.AddScoped<IWeatherSnapshotRepository, WeatherSnapshotRepository>();
    services.AddScoped<IDistrictRepository, DistrictRepository>();

    // ---------- Leaderboard ----------
    services.AddScoped<ILeaderboardStore, RedisLeaderboardStore>();

    // ---------- Background & infra helpers ----------
    services.AddSingleton<RedisLeaderElectionService>();
    services.AddScoped<LeaderboardRefreshJob>();

    // ---------- Forecast lookup (cache -> DB) ----------
    services.AddScoped<IForecastLookupService, ForecastLookupService>();

    // ---------- Daily forecast storage ----------
    services.AddScoped<IDailyDistrictForecastRepository, DailyDistrictForecastRepository>();

    // ---------- Redis daily forecast cache ----------
    services.AddScoped<IDailyForecastCache, RedisDailyForecastCache>();

    services.AddScoped<ISourceDistrictResolver, NearestNeighborSourceDistrictResolver>();

    services.AddScoped<IEventPublisher, HangfireEventPublisher>();

    services.AddScoped<IWeatherDataFetchJob, WeatherDataFetchJob>();

    services.AddTransient(typeof(HangfireEventJob<>));
    // services.AddScoped<IEventConsumer<WeatherDataBatchFetched>, DistrictRankingConsumer>();
    // services.AddScoped<IEventConsumer<WeatherDataBatchFetched>, DailyAggregationConsumer>();

    return services;
  }
}
