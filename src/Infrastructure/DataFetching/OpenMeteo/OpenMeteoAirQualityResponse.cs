internal sealed class OpenMeteoAirQualityResponse
{
    public HourlyAirQuality Hourly { get; set; } = null!;
}

internal sealed class HourlyAirQuality
{
    public List<string> Time { get; set; } = new();
    public List<double> Pm2_5 { get; set; } = new();
}