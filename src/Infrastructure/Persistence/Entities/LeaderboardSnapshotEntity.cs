namespace Infrastructure.Persistence.Entities;

public class LeaderboardSnapshotEntity
{
    public int Id { get; set; }

    public DateTime GeneratedAt { get; set; }

    public string PayloadJson { get; set; } = null!;
}