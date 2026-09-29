using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacationFinder.Core.Models;
using static System.Net.Mime.MediaTypeNames;

namespace VacationFinder.Cli.Input
{
    internal static class PreferencesPrompt
    {
        private static readonly (string Code, string Symbol)[] Curencies =
            {
                ("ILS", "₪"),
                ("USD", "$"),
                ("EUR", "€"),
                ("GBP", "£"),
            };
        public static UserPreferences Ask()
        {
            Console.WriteLine("=== Vacation Finder ===");
            Console.WriteLine();

            double minTemp = ConsoleInput.ReadDouble("Min prefered temperature for the trip C", -50, 50);
            double maxTemp = ConsoleInput.ReadDouble("Max prefered temperature for the trip C", minTemp, 50);

            int month = ConsoleInput.ReadMonth("Choose month for the trip ");

            Console.WriteLine("Curency:");
            for (int i = 0; i < Curencies.Length; i++)
            {
                Console.WriteLine($" {i + 1}. {Curencies[i].Code} ({Curencies[i].Symbol})");
            }
            int currencyChoice = ConsoleInput.ReadInt("Choose curency", 1, Curencies.Length);
            var currency = Curencies[currencyChoice - 1];

            decimal budget = ConsoleInput.ReadDecimal($"Budget in {currency.Code} ({currency.Symbol})", 1);
            int days = ConsoleInput.ReadInt("Enter The Amount Of Days You Plan To Go To Trip", 1, 365);

            string? country = ConsoleInput.OptionalText("Country (press Enter to skip)");
            string? continent = null;
            string[] ContinentList = new string[] { "Europe", "Asia", "Africa", "North America", "South America", "Oceania", null}; // null to ignore wich cont - anywhere in the world 
            if (country is null)
            {
                for(int i = 0; i < ContinentList.Length; i++)
                {
                    Console.WriteLine($"{i+1}. {ContinentList[i] ?? "Anywhere"}");
                }
                int UserInp = ConsoleInput.ReadInt("Choose One Of The Options, for continents: ", 1, 7);
                continent = ContinentList[UserInp-1];
            }

            return new UserPreferences(minTemp, maxTemp, month, budget, days, currency.Code, continent, country);
        }
    }
}
