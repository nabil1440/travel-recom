namespace Infrastructure.Persistence.Seed;

using System.Text.Json.Serialization;

internal sealed class DailyDistrictForecastSeedDto
{
    [JsonPropertyName("DistrictId")]
    public int DistrictId { get; set; }

    [JsonPropertyName("ForecastDate")]
    public string ForecastDate { get; set; } = null!;

    [JsonPropertyName("Temp2Pm")]
    public double Temp2Pm { get; set; }

    [JsonPropertyName("Pm25_2Pm")]
    public double Pm25_2Pm { get; set; }

    [JsonPropertyName("CreatedAt")]
    public string? CreatedAt { get; set; }
}

internal sealed class DailyDistrictForecastSeedRoot
{
    [JsonPropertyName("daily_district_forecasts")]
    public List<DailyDistrictForecastSeedDto> DailyDistrictForecasts { get; set; } = [];
}
