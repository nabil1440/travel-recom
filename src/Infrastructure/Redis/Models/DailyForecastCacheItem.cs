namespace Infrastructure.Redis.Models;

public sealed record DailyForecastCacheItem(
    int DistrictId,
    string DistrictName,
    DateOnly ForecastDate,
    double Temp2Pm,
    double Pm25_2Pm
);