namespace AppCore.Models;

public sealed record DistrictWeatherSnapshot(
    int DistrictId,
    DateOnly AggregationDate,
    double Temp2Pm,
    double Pm25_2Pm
);