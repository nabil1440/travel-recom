namespace Infrastructure.Persistence.Entities;

public class DistrictEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}