namespace AppCore.Abstractions.DataFetching;

using AppCore.Models;

public interface IDataFetchingService
{
    Task<RawWeatherForecast> GetWeatherForecastAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken);

    Task<RawAirQualityForecast> GetAirQualityForecastAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken);
}