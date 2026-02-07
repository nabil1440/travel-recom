namespace AppCore.Events;

public sealed record WeatherDataFetchedEvent(
    int DistrictId,
    string DistrictName,
    DateOnly Date,
    double Temperature2Pm,
    double Pm25_2Pm
);
