using Microsoft.Extensions.Configuration;
using System.Text;
using VacationFinder.Cli.Input;
using VacationFinder.Core.Models;
using VacationFinder.Infrastructure.OpenMeteo;

Console.OutputEncoding = Encoding.UTF8;
using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
var climateProvider = new OpenMeteoClimateProvider(httpClient);


UserPreferences prefs = PreferencesPrompt.Ask();
Console.WriteLine();
Console.WriteLine($"You asked for: {prefs}");