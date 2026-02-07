namespace AppCore.Abstractions.Services;

using AppCore.Events;
using AppCore.Models;

public interface IDistrictRankingService
{
    Task HandleWeatherBatchAsync(
        WeatherDataBatchFetched @event,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RankedDistrict>> GetTopDistrictsAsync(
        int count,
        CancellationToken cancellationToken
    );
}
