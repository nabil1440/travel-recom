namespace AppCore.Events;

public sealed record WeatherDataBatchFetched(
    string BatchId,
    DateTime FetchedAtUtc,
    IReadOnlyCollection<DistrictWeatherFacts> Districts
);

public sealed record DistrictWeatherFacts(
    int DistrictId,
    double Latitude,
    double Longitude,
    IReadOnlyCollection<DailyWeatherFact> Forecasts
);

public sealed record DailyWeatherFact(
    DateOnly Date,
    double Temp2Pm,
    double Pm25_2Pm
);