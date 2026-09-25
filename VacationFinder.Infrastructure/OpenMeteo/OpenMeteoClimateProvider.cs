using System.Globalization;
using System.Net.Http.Json;
using VacationFinder.Core.Interfaces;
using VacationFinder.Core.Models;

namespace VacationFinder.Infrastructure.OpenMeteo;

/// Gets a destination's typical climate for a month by averaging
/// the last few full years of historical data from Open Meteo
public class OpenMeteoClimateProvider : IClimateProvider
{
    private const string URL = "https://archive-api.open-meteo.com/v1/archive";
    private const int YearsToCount = 5;
    private const double RainyDayMM = 1.0; // a day with at least 1 mm counts as rainy
    private readonly HttpClient _httpClient;
    public OpenMeteoClimateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<ClimateInfo> GetClimateAsync(Destination destination, int month, CancellationToken ct = default) // got helped with claude. 
    {
        // validates the inputs builds the URL calls the API and passes the result on
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfLessThan(month, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(month, 12);

        string url = BuildUrl(destination.Latitude, destination.Longitude);

        OpenMeteoResponse? response = await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(url, ct);
        OpenMeteoDaily daily = response?.Daily
            ?? throw new InvalidOperationException($"Open-Meteo returned no daily data for {destination.CityName}.");

        return CalculateClimate(daily, month, destination.CityName);
    }
    private static string BuildUrl(double latitude, double longitude) // gets past 5 years, not including the curent year, cause its not the full info ( the year has not ended )
    {
        int lastFullYear = DateTime.UtcNow.Year - 1;
        int firstYear = lastFullYear - YearsToCount + 1;

        string lat = latitude.ToString(CultureInfo.InvariantCulture);
        string lon = longitude.ToString(CultureInfo.InvariantCulture);
        return $"{URL}?latitude={lat}&longitude={lon}" +
               $"&start_date={firstYear}-01-01&end_date={lastFullYear}-12-31" +
               "&daily=temperature_2m_mean,precipitation_sum&timezone=auto";
    }
    private static ClimateInfo CalculateClimate(OpenMeteoDaily daily, int month, string cityName)
    {
        double tempSum = 0;
        int tempCount = 0;
        int rainyDays = 0;

        // The three arrays should be the same length, but external data shouldnt be trusted 
        int length = Math.Min(daily.Time.Length, Math.Min(daily.TemperatureMean.Length, daily.PrecipitationSum.Length));

        // One pass over the data filter by month, sum temperatures, count rainy days
        for (int i = 0; i < length; i++)
        {
            if (daily.Time[i].Month != month)
                continue;

            if (daily.TemperatureMean[i] is double temp)
            {
                tempSum += temp;
                tempCount++;
            }

            if (daily.PrecipitationSum[i] is double rain && rain >= RainyDayMM)
                rainyDays++;
        }
        if (tempCount == 0) throw new InvalidOperationException($"No temperature data for {cityName} in month {month}.");
        double avgTemp = Math.Round(tempSum / tempCount, 1);
        int rainyDaysPerMonth = (int)Math.Round((double)rainyDays / YearsToCount);
        return new ClimateInfo(avgTemp, rainyDaysPerMonth);
    }
}