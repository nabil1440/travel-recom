namespace AppCore.Services;

using AppCore.Abstractions.Aggregation;
using AppCore.Models;

public sealed class WeatherAggregationService : IWeatherAggregationService
{
    private const int TargetUtcHour = 8; // 2 PM GMT+6

    public DistrictWeatherSnapshot Aggregate(
        District district,
        RawWeatherForecast weather,
        RawAirQualityForecast airQuality)
    {
        if (weather.Timestamps.Count == 0 || airQuality.Timestamps.Count == 0)
        {
            throw new InvalidOperationException("No forecast data available.");
        }

        var tempByDate = Extract2PmValues(
            weather.Timestamps,
            weather.Temperatures);

        var pm25ByDate = Extract2PmValues(
            airQuality.Timestamps,
            airQuality.Pm25Values);

        var commonDates = tempByDate.Keys
            .Intersect(pm25ByDate.Keys)
            .OrderBy(d => d)
            .ToList();

        if (commonDates.Count == 0)
        {
            throw new InvalidOperationException("No matching 2 PM data points found.");
        }

        var avgTemp = commonDates
            .Select(d => tempByDate[d])
            .Average();

        var avgPm25 = commonDates
            .Select(d => pm25ByDate[d])
            .Average();

        // Date here represents "aggregation date"
        var aggregationDate = commonDates.First();

        return new DistrictWeatherSnapshot(
            district.Id,
            aggregationDate,
            avgTemp,
            avgPm25
        );
    }

    private static Dictionary<DateOnly, double> Extract2PmValues(
        IReadOnlyList<DateTime> timestamps,
        IReadOnlyList<double> values)
    {
        var result = new Dictionary<DateOnly, double>();

        for (int i = 0; i < timestamps.Count; i++)
        {
            var utcTime = timestamps[i];

            if (utcTime.Hour != TargetUtcHour)
                continue;

            var date = DateOnly.FromDateTime(utcTime);

            // One value per day at 2 PM local
            if (!result.ContainsKey(date))
            {
                result[date] = values[i];
            }
        }

        return result;
    }
}