namespace Api.Extensions;

using Infrastructure.Persistence;
using Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
{
    using var scope = app.ApplicationServices.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();

    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

    var seedPath = Path.Combine(
        env.ContentRootPath,
        "Seed",
        "districts.bd.json");

    await DistrictSeeder.SeedAsync(db, seedPath);
}
}