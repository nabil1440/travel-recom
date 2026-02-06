namespace AppCore.Abstractions.Aggregation;

using AppCore.Models;

public interface IWeatherAggregationService
{
    DistrictWeatherSnapshot Aggregate(
        District district,
        RawWeatherForecast weather,
        RawAirQualityForecast airQuality);
}