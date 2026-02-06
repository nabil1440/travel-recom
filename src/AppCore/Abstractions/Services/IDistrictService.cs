namespace AppCore.Abstractions.Services;

using AppCore.Models;

public interface IDistrictService
{
    Task<IReadOnlyCollection<District>> GetDistrictsAsync(
        CancellationToken cancellationToken);
}