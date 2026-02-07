namespace Infrastructure.Persistence.Seed;

using System.Text.Json.Serialization;

internal sealed class DistrictWeatherSnapshotSeedDto
{
    [JsonPropertyName("DistrictId")]
    public int DistrictId { get; set; }

    [JsonPropertyName("date")]
    public string AggregationDate { get; set; } = null!;

    [JsonPropertyName("Temp2Pm")]
    public double Temp2Pm { get; set; }

    [JsonPropertyName("Pm25_2Pm")]
    public double Pm25_2Pm { get; set; }

    [JsonPropertyName("CreatedAt")]
    public string? CreatedAt { get; set; }
}

internal sealed class DistrictWeatherSnapshotSeedRoot
{
    [JsonPropertyName("district_weather_snapshots")]
    public List<DistrictWeatherSnapshotSeedDto> DistrictWeatherSnapshots { get; set; } = [];
}
