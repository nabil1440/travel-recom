namespace AppCore.Services;

using AppCore.Abstractions.Persistence;
using AppCore.Abstractions.Services;
using AppCore.Models;

public sealed class DistrictService : IDistrictService
{
    private readonly IDistrictRepository _districtRepository;

    public DistrictService(IDistrictRepository districtRepository)
    {
        _districtRepository = districtRepository;
    }

    public async Task<IReadOnlyCollection<District>> GetDistrictsAsync(
        CancellationToken cancellationToken)
    {
        var districts = await _districtRepository
            .GetAllAsync(cancellationToken);

        if (districts.Count == 0)
            throw new Exception("Districts are not available.");

        return districts;
    }
}