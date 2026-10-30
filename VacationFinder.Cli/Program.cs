using System.Text;
using System.Text.Json;
using VacationFinder.Cli.Input;
using VacationFinder.Core.Models;
using VacationFinder.Core.Scoring;
using VacationFinder.Infrastructure.OpenMeteo;

Console.OutputEncoding = Encoding.UTF8;

UserPreferences prefs = PreferencesPrompt.Ask();

// loading the destinations from your jsn file
string path = Path.Combine(AppContext.BaseDirectory,"Data","destinations.json");
List<Destination> destinations;

try
{
    destinations = LoadDestinations(path);
}
catch (Exception ex) when (
    ex is IOException or UnauthorizedAccessException or JsonException)
{
    Console.WriteLine($"Could not load destinations: {ex.Message}");
    return;
}

// 3. Filter by country or continent.
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

// Fixed example rates for the demo; update these manually.
// Each value is the number of currency units per 1 USD.
decimal currencyRate = prefs.Currency switch
{
    "USD" => 1m,
    "ILS" => 3.70m,
    "EUR" => 0.92m,
    "GBP" => 0.79m,
    _ => throw new ArgumentException("Unsupported currency.")
};

decimal budgetUsd = prefs.Budget / currencyRate;

using var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30)
};

var climateProvider = new OpenMeteoClimateProvider(httpClient);
var results = new List<ScoredResult>();

Console.WriteLine();
Console.WriteLine("Searching...");

// 4. Check costs, fetch weather, and calculate scores.
foreach (Destination destination in matches)
{
    decimal costUsd =
        destination.FlightCostUsd +
        destination.DailyCostUsd * prefs.Days;

    // Avoid weather requests for destinations over budget.
    if (costUsd > budgetUsd)
        continue;

    Console.WriteLine($"Checking {destination.CityName}...");

    try
    {
        ClimateInfo climate = await climateProvider.GetClimateAsync(
            destination,
            prefs.Month);

        double tempScore = Scorer.TemperatureScore(
            climate.AvgTemp,
            prefs.MinTemp,
            prefs.MaxTemp);

        double priceScore = Scorer.PriceScore(
            costUsd,
            budgetUsd);

        // Equal importance: 50% temperature, 50% price.
        double finalScore = Scorer.FinalScore(
            tempScore,
            priceScore,
            0.5,
            0.5);

        decimal costInUserCurrency = costUsd * currencyRate;

        results.Add(new ScoredResult(
            destination,
            climate,
            costInUserCurrency,
            finalScore));
    }
    catch (Exception ex) when (
        ex is HttpRequestException
        or TaskCanceledException
        or InvalidOperationException
        or JsonException)
    {
        Console.WriteLine(
            $"Could not check {destination.CityName}: {ex.Message}");
    }
}

// 5. Print results, highest score first.
if (results.Count == 0)
{
    Console.WriteLine(
        "No affordable destinations with available weather data.");
    return;
}

List<ScoredResult> rankedResults = new List<ScoredResult>(results);
rankedResults.Sort(CompareResults);

Console.WriteLine();
Console.WriteLine("Results — estimated costs:");
Console.WriteLine();

Console.WriteLine(
    $"{"#",3} {"Country",-22} {"City",-18} " +
    $"{"Avg °C",8} {"Cost",14} {"Rating",10}");

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

// Reads your existing JSON, including the two price fields.
static List<Destination> LoadDestinations(string path)
{
    using JsonDocument document = JsonDocument.Parse(
        File.ReadAllText(path));

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

// Country takes priority. If neither is selected, include everywhere.
static bool MatchesLocation(
    Destination destination,
    UserPreferences prefs)
{
    if (!string.IsNullOrWhiteSpace(prefs.Country))
    {
        return string.Equals(
            destination.Country,
            prefs.Country.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }

    if (!string.IsNullOrWhiteSpace(prefs.Continent))
    {
        return string.Equals(
            destination.Continent,
            prefs.Continent.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }

    return true;
}



static int CompareResults(ScoredResult first, ScoredResult second)
{
    // Higher scores come first.
    int comparison = second.FinalScore.CompareTo(first.FinalScore);

    if (comparison != 0)
    {
        return comparison;
    }

    // Equal scores: sort alphabetically by country.
    comparison = string.Compare(
        first.Destination.Country,
        second.Destination.Country);

    if (comparison != 0)
    {
        return comparison;
    }

    // Same country and score: sort alphabetically by city.
    return string.Compare(
        first.Destination.CityName,
        second.Destination.CityName);
}