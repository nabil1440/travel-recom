namespace AppCore.Models;

public sealed record RawAirQualityForecast(
    IReadOnlyList<DateTime> Timestamps,
    IReadOnlyList<double> Pm25Values
);