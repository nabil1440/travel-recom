namespace AppCore.Abstractions.Persistence;

using AppCore.Models;

public interface IWeatherSnapshotRepository
{
    Task SaveAsync(
        IReadOnlyCollection<DistrictWeatherSnapshot> snapshots,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DistrictWeatherSnapshot>>
        GetLatestAsync(CancellationToken cancellationToken);
}