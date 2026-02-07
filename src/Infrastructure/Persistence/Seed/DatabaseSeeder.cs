namespace Infrastructure.Persistence.Seed;

using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed record SeedFilePaths(
    string DistrictsPath,
    string WeatherSnapshotsPath,
    string DailyForecastsPath);

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        SeedFilePaths paths,
        CancellationToken cancellationToken = default)
    {
        if (await db.Districts.AnyAsync(cancellationToken))
            return;

        if (await db.DistrictWeatherSnapshots.AnyAsync(cancellationToken))
            return;

        if (await db.DailyDistrictForecasts.AnyAsync(cancellationToken))
            return;

        await DistrictSeeder.SeedAsync(db, paths.DistrictsPath, cancellationToken);
        await DistrictWeatherSnapshotSeeder.SeedAsync(db, paths.WeatherSnapshotsPath, cancellationToken);
        await DailyDistrictForecastSeeder.SeedAsync(db, paths.DailyForecastsPath, cancellationToken);
    }
}
