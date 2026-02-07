namespace AppCore.Models;

public sealed record DailyDistrictForecast(
    int DistrictId,
    DateOnly ForecastDate,
    double Temp2Pm,
    double Pm25_2Pm
);