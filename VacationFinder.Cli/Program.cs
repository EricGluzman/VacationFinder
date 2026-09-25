using System.Text;
using VacationFinder.Core.Models;
using VacationFinder.Infrastructure.OpenMeteo;

Console.OutputEncoding = Encoding.UTF8;
using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
var climateProvider = new OpenMeteoClimateProvider(httpClient);


var capeTown = new Destination("Cape Town", "South Africa", "Africa", -33.92, 18.42);// test
var testCases = new (Destination City, int Month)[]
{
    (new Destination("Barcelona", "Spain", "Europe", 41.39, 2.17), 4), // test 
    (capeTown, 1),
    (capeTown, 12)
};

foreach (var (city, month) in testCases)
{
    try
    {
        ClimateInfo climate = await climateProvider.GetClimateAsync(city, month);
        Console.WriteLine($"{city.CityName,-10} month {month,2}: {climate.AvgTemp,5}°C ~{climate.NumberOfRainyDays} rainy days");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"{city.CityName}: network or API error: {ex.Message}");
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine($"{city.CityName}: request timed out.");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"{city.CityName}: bad data: {ex.Message}");
    }
}