namespace Infrastructure.Persistence.Repositories;

using Infrastructure.Persistence.Entities;

public interface IDailyDistrictForecastRepository
{
    Task<DailyDistrictForecastEntity?> GetAsync(
        int districtId,
        DateOnly date,
        CancellationToken cancellationToken);

    Task SaveAsync(
        IEnumerable<DailyDistrictForecastEntity> entities,
        CancellationToken cancellationToken);
}