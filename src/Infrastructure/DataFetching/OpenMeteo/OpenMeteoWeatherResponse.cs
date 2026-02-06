internal sealed class OpenMeteoWeatherResponse
{
    public HourlyWeather Hourly { get; set; } = null!;
}

internal sealed class HourlyWeather
{
    public List<string> Time { get; set; } = new();
    public List<double> Temperature_2m { get; set; } = new();
}