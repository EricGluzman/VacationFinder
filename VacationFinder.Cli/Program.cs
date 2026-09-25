using System.Text;
using VacationFinder.Core.Models;
using VacationFinder.Core.Scoring;

namespace VacationFinder.Cli;

internal static class Program
{
    private static void Main()
    {
        // THIS CODE ( THE MAIN ) IS MADE BY CLAUDE CODE. JUST TO TEST THE WORKING STATE OF SCORER METHODS. 
        Console.OutputEncoding = Encoding.UTF8; 
        var preferences = new UserPreferences(
            minTemp: 14,
            maxTemp: 22,
            month: 6,
            budget: 5000m,
            days: 2);

        List<Destination> destinations = GetDestinations();
        Dictionary<string, FakeCityData> fakeData = GetFakeData();

        List<ScoredResult> results = ScoreDestinations(destinations, fakeData, preferences);

        PrintResults(results, preferences);
    }
    private static List<ScoredResult> ScoreDestinations(
        List<Destination> destinations,
        Dictionary<string, FakeCityData> fakeData,
        UserPreferences prefs)
    {
        return destinations
            .Where(d => MatchesLocation(d, prefs))
            .Where(d => fakeData.ContainsKey(d.CityName))   // skip cities we have no data for
            .Select(d => ScoreOne(d, fakeData[d.CityName], prefs))
            .OrderByDescending(r => r.FinalScore)
            .ToList();
    }

    private static ScoredResult ScoreOne(Destination destination, FakeCityData data, UserPreferences prefs)
    {
        decimal totalCost = data.FlightCost + data.DailyCost * prefs.Days;

        double tempScore = Scorer.TemperatureScore(data.Climate.AvgTemp, prefs.MinTemp, prefs.MaxTemp);
        double priceScore = Scorer.PriceScore(totalCost, prefs.Budget);
        double finalScore = Scorer.FinalScore(tempScore, priceScore, tempWeight: 0, priceWeight: 0); // 0,0 = use defaults

        return new ScoredResult(destination, data.Climate, totalCost, finalScore);
    }

    private static bool MatchesLocation(Destination destination, UserPreferences prefs)
    {
        if (prefs.Country is not null)
            return string.Equals(destination.Country, prefs.Country, StringComparison.OrdinalIgnoreCase);

        if (prefs.Continent is not null)
            return string.Equals(destination.Continent, prefs.Continent, StringComparison.OrdinalIgnoreCase);

        return true;
    }
    private static void PrintResults(List<ScoredResult> results, UserPreferences prefs)
    {
        string location = prefs.Country ?? prefs.Continent ?? "anywhere";
        Console.WriteLine($"Looking for {prefs.MinTemp}-{prefs.MaxTemp}°C in month {prefs.Month}, " +
                          $"{location}, budget {prefs.Budget:N0}₪ for {prefs.Days} days");
        Console.WriteLine();

        if (results.Count == 0)
        {
            Console.WriteLine("No destinations match your search.");
            return;
        }

        Console.WriteLine($"{"#",-3} {"City",-12} {"Country",-14} {"Temp",6} {"Cost",10} {"Score",8}");
        Console.WriteLine(new string('-', 58));

        int rank = 1;
        foreach (ScoredResult r in results)
        {
            Console.WriteLine(
                $"{rank,-3} {r.Destination.CityName,-12} {r.Destination.Country,-14} " +
                $"{r.Climate.AvgTemp,5:F0}° {r.TotalCost,9:N0}₪ {r.FinalScore * 10,5:F1}/10");
            rank++;
        }
    }
    private static List<Destination> GetDestinations() => new()
    {
        new("Athens",    "Greece",       "Europe",        37.98,   23.73),
        new("Rome",      "Italy",        "Europe",        41.90,   12.50),
        new("Barcelona", "Spain",        "Europe",        41.39,    2.17),
        new("Lisbon",    "Portugal",     "Europe",        38.72,   -9.14),
        new("Tbilisi",   "Georgia",      "Asia",          41.72,   44.79),
        new("Dubai",     "UAE",          "Asia",          25.20,   55.27),
        new("Bangkok",   "Thailand",     "Asia",          13.76,  100.50),
        new("Tokyo",     "Japan",        "Asia",          35.68,  139.69),
        new("Marrakech", "Morocco",      "Africa",        31.63,   -7.99),
        new("Cape Town", "South Africa", "Africa",       -33.92,   18.42),
        new("Cancun",    "Mexico",       "North America", 21.16,  -86.85),
        new("New York",  "USA",          "North America", 40.71,  -74.01),
    };
    private static Dictionary<string, FakeCityData> GetFakeData() => new()
    {
        ["Athens"] = new(new ClimateInfo(17, 5), DailyCost: 450m, FlightCost: 900m),
        ["Rome"] = new(new ClimateInfo(15, 8), DailyCost: 600m, FlightCost: 1200m),
        ["Barcelona"] = new(new ClimateInfo(16, 6), DailyCost: 650m, FlightCost: 1400m),
        ["Lisbon"] = new(new ClimateInfo(16, 9), DailyCost: 500m, FlightCost: 1800m),
        ["Tbilisi"] = new(new ClimateInfo(13, 8), DailyCost: 300m, FlightCost: 900m),
        ["Dubai"] = new(new ClimateInfo(30, 1), DailyCost: 800m, FlightCost: 1300m),
        ["Bangkok"] = new(new ClimateInfo(30, 4), DailyCost: 350m, FlightCost: 3000m),
        ["Tokyo"] = new(new ClimateInfo(15, 10), DailyCost: 700m, FlightCost: 4500m),
        ["Marrakech"] = new(new ClimateInfo(20, 4), DailyCost: 350m, FlightCost: 2000m),
        ["Cape Town"] = new(new ClimateInfo(20, 5), DailyCost: 450m, FlightCost: 3800m),
        ["Cancun"] = new(new ClimateInfo(27, 3), DailyCost: 600m, FlightCost: 4500m),
        ["New York"] = new(new ClimateInfo(12, 11), DailyCost: 1100m, FlightCost: 3500m),
    };
    private record FakeCityData(ClimateInfo Climate, decimal DailyCost, decimal FlightCost);
}