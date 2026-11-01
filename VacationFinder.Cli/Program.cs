using System.Text;
using System.Text.Json;
using VacationFinder.Cli.Input;
using VacationFinder.Core.Models;
using VacationFinder.Core.Scoring;
using VacationFinder.Infrastructure.OpenMeteo;

Console.OutputEncoding = Encoding.UTF8;

UserPreferences prefs = PreferencesPrompt.Ask();

// loading the destinations from jsn file
string path = Path.Combine(AppContext.BaseDirectory,"Data","destinations.json");
List<Destination> destinations;

try
{
    destinations = LoadDestinations(path);
} // IOE for missing file, 2 one is premission, json syntax 
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
{
    Console.WriteLine($"Could not load destinations: {ex.Message}");
    return;
}

// filter by country or cont
List<Destination> matches = new List<Destination>();

foreach (Destination destination in destinations)
{
    if (MatchesLocation(destination, prefs))
    {
        matches.Add(destination);
    }
}
if (matches.Count == 0)
{
    Console.WriteLine("No destinations match your location.");
    return;
}

// Fixed example rates for the demo i`ll update those manualy
// Each value is the number of currency units per 1 usd
decimal currencyRate = prefs.Currency switch
{
    "USD" => 1m,
    "ILS" => 3.20m,
    "EUR" => 0.92m,
    "GBP" => 0.79m,
    _ => throw new ArgumentException("Unsupported currency.")
};

decimal budgetUsd = prefs.Budget / currencyRate;
// creating http client 
using var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30)
};

var climateProvider = new OpenMeteoClimateProvider(httpClient);
var results = new List<ScoredResult>();

Console.WriteLine();
Console.WriteLine("Searching...");

//  Check costs fetch weather and calculate scores
foreach (Destination destination in matches)
{
    decimal costUsd = destination.FlightCostUsd + destination.DailyCostUsd * prefs.Days;

    // avoinding requsting the countries or cont that over the budget ( i think i`ll change thath in the future, to even show those
    // over the budget ones. )
    if (costUsd > budgetUsd)
        continue;

    Console.WriteLine($"Checking {destination.CityName}...");

    try
    {
        ClimateInfo climate = await climateProvider.GetClimateAsync(destination,prefs.Month);

        double tempScore = Scorer.TemperatureScore(climate.AvgTemp,prefs.MinTemp,prefs.MaxTemp);

        double priceScore = Scorer.PriceScore(costUsd,budgetUsd);

        double finalScore = Scorer.FinalScore(tempScore,priceScore,0.5,0.5); // for now i`ll leave the weight of cost and temp equal.

        decimal costInUserCurrency = costUsd * currencyRate;

        results.Add(new ScoredResult(destination,climate,costInUserCurrency,finalScore));
    }
    catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException or JsonException)
    {
        Console.WriteLine($"Could not check {destination.CityName}: {ex.Message}");
    }
}

// Printing results high ones come first
if (results.Count == 0)
{
    Console.WriteLine("No affordable destinations with available weather data.");
    return;
}

List<ScoredResult> rankedResults = new List<ScoredResult>(results);
rankedResults.Sort(CompareResults);

Console.WriteLine();
Console.WriteLine("Results — estimated costs:");
Console.WriteLine();
// with claudes help 

Console.WriteLine($"{"#",3} {"Country",-22} {"City",-18} " + $"{"Avg °C",8} {"Cost",14} {"Rating",10}");
Console.WriteLine(new string('-', 82));

int rank = 1;

foreach (ScoredResult result in rankedResults)
{
    string cost = $"{result.TotalCost:N0} {prefs.Currency}";

    Console.WriteLine(
        $"{rank,3} " +
        $"{result.Destination.Country,-22} " +
        $"{result.Destination.CityName,-18} " +
        $"{result.Climate.AvgTemp,8:F1} " +
        $"{cost,14} " +
        $"{result.FinalScore * 10,7:F1}/10");
    rank++;
}

//

static List<Destination> LoadDestinations(string path)
{
    using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));

    var destinations = new List<Destination>();

    foreach (JsonElement item in document.RootElement.EnumerateArray())
    {
        var destination = new Destination(
            item.GetProperty("city").GetString()!,
            item.GetProperty("country").GetString()!,
            item.GetProperty("countryCode").GetString()!,
            item.GetProperty("continent").GetString()!,
            item.GetProperty("lat").GetDouble(),
            item.GetProperty("lng").GetDouble(),
            item.GetProperty("flightCostUsd").GetDecimal(),
            item.GetProperty("dailyCostUsd").GetDecimal());

        destinations.Add(destination);
    }

    return destinations;
}

// first choose country if both are null then anywhere 
static bool MatchesLocation(Destination destination,UserPreferences prefs)
{
    if (!string.IsNullOrWhiteSpace(prefs.Country))
    {
        return string.Equals(destination.Country,prefs.Country.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    if (!string.IsNullOrWhiteSpace(prefs.Continent))
    {
        return string.Equals(destination.Continent,prefs.Continent.Trim(), StringComparison.OrdinalIgnoreCase);
    }
    return true;
}

static int CompareResults(ScoredResult first, ScoredResult second)
{
    // Higher scores come first
    int comparison = second.FinalScore.CompareTo(first.FinalScore);

    if (comparison != 0)
    {
        return comparison;
    }

    // for equal ratings we score by abc
    comparison = string.Compare(first.Destination.Country,second.Destination.Country);

    if (comparison != 0)
    {
        return comparison;
    }

    // for same country with same rating we sort by abc for the country
    return string.Compare(first.Destination.CityName,second.Destination.CityName);
}