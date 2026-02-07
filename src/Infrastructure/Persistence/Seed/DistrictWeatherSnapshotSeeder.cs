namespace Infrastructure.Persistence.Seed;

using System.Globalization;
using System.Text.Json;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

public static class DistrictWeatherSnapshotSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        string jsonPath,
        CancellationToken cancellationToken = default)
    {
        if (await db.DistrictWeatherSnapshots.AnyAsync(cancellationToken))
            return;

        if (!File.Exists(jsonPath))
            throw new FileNotFoundException($"District weather snapshot seed file not found at {jsonPath}");

        var json = await File.ReadAllTextAsync(jsonPath, cancellationToken);

        var root =
            JsonSerializer.Deserialize<DistrictWeatherSnapshotSeedRoot>(json)
            ?? throw new InvalidOperationException("Invalid district weather snapshot seed file");

        var entities = root.DistrictWeatherSnapshots.Select(dto =>
            new DistrictWeatherSnapshotEntity
            {
                DistrictId = dto.DistrictId,
                AggregationDate = DateOnly.Parse(dto.AggregationDate, CultureInfo.InvariantCulture),
                Temp2Pm = dto.Temp2Pm,
                Pm25_2Pm = dto.Pm25_2Pm,
                CreatedAt = ParseCreatedAt(dto.CreatedAt)
            })
            .ToList();

        db.DistrictWeatherSnapshots.AddRange(entities);
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
