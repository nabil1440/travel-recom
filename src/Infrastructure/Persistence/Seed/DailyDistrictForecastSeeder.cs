namespace Infrastructure.Persistence.Seed;

using System.Globalization;
using System.Text.Json;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

public static class DailyDistrictForecastSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        string jsonPath,
        CancellationToken cancellationToken = default)
    {
        if (await db.DailyDistrictForecasts.AnyAsync(cancellationToken))
            return;

        if (!File.Exists(jsonPath))
            throw new FileNotFoundException($"Daily district forecast seed file not found at {jsonPath}");

        var json = await File.ReadAllTextAsync(jsonPath, cancellationToken);

        var root =
            JsonSerializer.Deserialize<DailyDistrictForecastSeedRoot>(json)
            ?? throw new InvalidOperationException("Invalid daily district forecast seed file");

        var entities = root.DailyDistrictForecasts.Select(dto =>
            new DailyDistrictForecastEntity
            {
                DistrictId = dto.DistrictId,
                ForecastDate = DateOnly.Parse(dto.ForecastDate, CultureInfo.InvariantCulture),
                Temp2Pm = dto.Temp2Pm,
                Pm25_2Pm = dto.Pm25_2Pm,
                CreatedAt = ParseCreatedAt(dto.CreatedAt)
            })
            .ToList();

        db.DailyDistrictForecasts.AddRange(entities);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static DateTime ParseCreatedAt(string? createdAt)
    {
        if (string.IsNullOrWhiteSpace(createdAt))
            return DateTime.UtcNow;

        return DateTime.Parse(
            createdAt,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);
    }
}
