using Infrastructure.Redis;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Persistence;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/health", async (
    [FromServices] AppDbContext db,
    [FromServices] RedisConnection redis) =>
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
        var redisDb = redis.Connection.GetDatabase();
        await redisDb.PingAsync();
        checks["redis"] = "ok";
    }
    catch
    {
        checks["redis"] = "down";
        overallStatus = "degraded";
    }

    return Results.Ok(new
    {
        status = overallStatus,
        checks
    });
});

app.Run();