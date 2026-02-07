using Api.Extensions;
using Api.Middleware;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure;
using Infrastructure.BackgroundJobs;
using Infrastructure.Jobs;
using Infrastructure.Persistence;
using Infrastructure.Redis;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("Postgres"))
);

builder.Services.AddHangfireServer();

var app = builder.Build();

await app.ApplyMigrationsAsync();

using (var scope = app.Services.CreateScope())
{
    var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobs.AddOrUpdate<WeatherDataFetchJob>(
        "weather-data-fetch",
        job => job.ExecuteAsync(CancellationToken.None),
        Cron.Hourly
    );

    var backgroundJobs = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();

    backgroundJobs.Enqueue<WeatherDataFetchJob>(job => job.ExecuteAsync(CancellationToken.None));
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHangfireDashboard("/hangfire");

app.MapControllers();

app.MapGet(
    "/health",
    async ([FromServices] AppDbContext db, [FromServices] IConnectionMultiplexer redis) =>
    {
        var checks = new Dictionary<string, string>();
        var overallStatus = "ok";

        // API is up if we reached here
        checks["api"] = "ok";

        // Database check
        try
        {
            var canConnect = await db.Database.CanConnectAsync();
            checks["database"] = canConnect ? "ok" : "down";

            if (!canConnect)
                overallStatus = "degraded";
        }
        catch
        {
            checks["database"] = "down";
            overallStatus = "degraded";
        }

        // Redis check
        try
        {
            var redisDb = redis.GetDatabase();
            await redisDb.PingAsync();
            checks["redis"] = "ok";
        }
        catch
        {
            checks["redis"] = "down";
            overallStatus = "degraded";
        }

        return Results.Ok(new { status = overallStatus, checks });
    }
);

app.Run();
