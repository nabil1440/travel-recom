using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<DistrictEntity> Districts => Set<DistrictEntity>();

    public DbSet<DistrictWeatherSnapshotEntity> DistrictWeatherSnapshots =>
        Set<DistrictWeatherSnapshotEntity>();

    public DbSet<LeaderboardSnapshotEntity> LeaderboardSnapshots =>
        Set<LeaderboardSnapshotEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DistrictEntity>(entity =>
        {
            entity.ToTable("districts");
            entity.HasKey(d => d.Id);
        });

        modelBuilder.Entity<DistrictWeatherSnapshotEntity>(entity =>
        {
            entity.ToTable("district_weather_snapshots");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.DistrictId, e.Date }).IsUnique();

            entity.HasOne(e => e.District).WithMany().HasForeignKey(e => e.DistrictId);
        });

        modelBuilder.Entity<LeaderboardSnapshotEntity>(entity =>
        {
            entity.ToTable("leaderboard_snapshots");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.GeneratedAt);
        });
    }
}
