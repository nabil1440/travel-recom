namespace Infrastructure.Services;

using AppCore.Abstractions.Services;
using AppCore.Models;

public sealed class NearestNeighborSourceDistrictResolver : ISourceDistrictResolver
{
    private readonly IDistrictService _districtService;

    public NearestNeighborSourceDistrictResolver(
        IDistrictService districtService)
    {
        _districtService = districtService;
    }

    public async Task<SourceDistrictResolutionResult> ResolveAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        var districts = await _districtService.GetDistrictsAsync(cancellationToken);

        if (districts.Count == 0)
        {
            return SourceDistrictResolutionResult.NotFound();
        }

        var nearest = districts
            .Select(d => new
            {
                d.Id,
                Distance = Haversine(
                    latitude,
                    longitude,
                    d.Latitude,
                    d.Longitude)
            })
            .OrderBy(x => x.Distance)
            .First();

        return SourceDistrictResolutionResult.Success(nearest.Id);
    }

    // Distance in kilometers
    private static double Haversine(
        double lat1,
        double lon1,
        double lat2,
        double lon2)
    {
        const double R = 6371; // Earth radius in km

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(DegreesToRadians(lat1)) *
            Math.Cos(DegreesToRadians(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private static double DegreesToRadians(double degrees)
        => degrees * (Math.PI / 180);
}