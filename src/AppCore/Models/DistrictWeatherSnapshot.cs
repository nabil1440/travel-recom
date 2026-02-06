namespace AppCore.Models;

public sealed record DistrictWeatherSnapshot(
    int DistrictId,
    DateOnly Date,
    double Temp2Pm,
    double Pm25_2Pm
);