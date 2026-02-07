namespace AppCore.Events;
public sealed record WeatherDataBatchFetched(
    DateTime FetchedAtUtc,
    IReadOnlyCollection<DistrictWeatherFacts> Districts,
    string? BatchId = null
);

public sealed record DistrictWeatherFacts(
    int DistrictId,
    IReadOnlyCollection<DailyWeatherFact> Forecasts
);

public sealed record DailyWeatherFact(
    DateOnly Date,
    double Temp2Pm,
    double Pm25_2Pm
);