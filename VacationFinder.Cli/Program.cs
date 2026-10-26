using System.Text;
using VacationFinder.Cli.Input;
using VacationFinder.Core.Models;
using VacationFinder.Infrastructure.Json;

Console.OutputEncoding = Encoding.UTF8;

UserPreferences prefs = PreferencesPrompt.Ask();
Console.WriteLine();

// The file sits next to the .exe (copied there at build time), so build the path from BaseDirectory.
string path = Path.Combine(AppContext.BaseDirectory, "Data", "destinations.json");
var destinationSource = new JsonDestinationSource(path);

IReadOnlyList<Destination> all;
try
{
    all = await destinationSource.GetDestinationsAsync();
}
catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException or System.Text.Json.JsonException)
{
    Console.WriteLine($"Could not load destinations: {ex.Message}");
    return;
}

List<Destination> matches = all.Where(d => MatchesLocation(d, prefs)).ToList();

string location = prefs.Country ?? prefs.Continent ?? "anywhere";
Console.WriteLine($"Loaded {all.Count} destinations, {matches.Count} match \"{location}\":");
Console.WriteLine();

if (matches.Count == 0)
{
    Console.WriteLine("No destinations match. Check the spelling of the country.");
    return;
}

foreach (Destination d in matches)
{
    Console.WriteLine($"  {d.CityName,-16} {d.Country,-22} {d.Continent,-14} ({d.Latitude}, {d.Longitude})");
}
// Three possible states, guaranteed by the UserPreferences constructor:
// country set, continent set, or neither (= anywhere).
static bool MatchesLocation(Destination destination, UserPreferences prefs)
{
    if (prefs.Country is not null)
        return destination.Country.Equals(prefs.Country, StringComparison.OrdinalIgnoreCase);

    if (prefs.Continent is not null)
        return destination.Continent.Equals(prefs.Continent, StringComparison.OrdinalIgnoreCase);

    return true;
}