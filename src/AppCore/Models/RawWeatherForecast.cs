namespace AppCore.Models;

public sealed record RawWeatherForecast(
    IReadOnlyList<DateTime> Timestamps,
    IReadOnlyList<double> Temperatures
);