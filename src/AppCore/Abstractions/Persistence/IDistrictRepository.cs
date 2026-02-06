namespace AppCore.Abstractions.Persistence;

using AppCore.Models;

public interface IDistrictRepository
{
    Task<IReadOnlyCollection<District>> GetAllAsync(
        CancellationToken cancellationToken);
}