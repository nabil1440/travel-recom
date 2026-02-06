namespace Infrastructure.Persistence.Seed;

using System.Globalization;
using System.Text.Json;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

public static class DistrictSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        string jsonPath,
        CancellationToken cancellationToken = default
    )
    {
        if (await db.Districts.AnyAsync(cancellationToken))
            return;

        if (!File.Exists(jsonPath))
            throw new FileNotFoundException($"District seed file not found at {jsonPath}");

        var json = await File.ReadAllTextAsync(jsonPath, cancellationToken);

        var root =
            JsonSerializer.Deserialize<DistrictSeedRoot>(json)
            ?? throw new InvalidOperationException("Invalid district seed file");

        var dtos = root.Districts;

        var districts = dtos.Select(dto => new DistrictEntity
            {
                Id = int.Parse(dto.Id, CultureInfo.InvariantCulture),
                Name = dto.Name,
                Latitude = double.Parse(dto.Latitude, CultureInfo.InvariantCulture),
                Longitude = double.Parse(dto.Longitude, CultureInfo.InvariantCulture)
            })
            .ToList();

        db.Districts.AddRange(districts);
        await db.SaveChangesAsync(cancellationToken);
    }
}
