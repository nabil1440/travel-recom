namespace AppCore.Abstractions.Services;

using AppCore.Models;

public interface ISourceDistrictResolver
{
    Task<SourceDistrictResolutionResult> ResolveAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken);
}