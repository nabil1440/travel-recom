namespace AppCore.Abstractions.Services;

using AppCore.Models;

public interface IForecastLookupService
{
    Task<ForecastLookupResult> GetForecastAsync(
        int districtId,
        DateOnly date,
        CancellationToken cancellationToken);
}