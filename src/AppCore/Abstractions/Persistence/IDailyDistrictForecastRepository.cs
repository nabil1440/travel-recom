namespace AppCore.Abstractions.Persistence;

using AppCore.Models;

public interface IDailyDistrictForecastRepository
{
    Task<DailyDistrictForecast?> GetAsync(
        int districtId,
        DateOnly date,
        CancellationToken cancellationToken);

    Task SaveAsync(
        IEnumerable<DailyDistrictForecast> forecasts,
        CancellationToken cancellationToken);
}
