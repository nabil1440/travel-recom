namespace AppCore.Models;

public sealed record SourceDistrictResolutionResult(
    bool Found,
    int? DistrictId
)
{
    public static SourceDistrictResolutionResult NotFound()
        => new(false, null);

    public static SourceDistrictResolutionResult Success(int districtId)
        => new(true, districtId);
}