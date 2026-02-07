namespace Infrastructure.Persistence.Entities;

public class DistrictWeatherSnapshotEntity
{
    public int Id { get; set; }

    public int DistrictId { get; set; }

    public DateOnly AggregationDate { get; set; }

    public double Temp2Pm { get; set; }

    public double Pm25_2Pm { get; set; }

    public DateTime CreatedAt { get; set; }

    public DistrictEntity District { get; set; } = null!;
}