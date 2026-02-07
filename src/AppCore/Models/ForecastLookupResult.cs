namespace AppCore.Models;

public sealed record ForecastLookupResult(
    bool Found,
    DailyDistrictForecast? Forecast
)
{
    public static ForecastLookupResult NotFound()
        => new(false, null);

    public static ForecastLookupResult Success(DailyDistrictForecast forecast)
        => new(true, forecast);
}