using System.Globalization;

namespace VacationFinder.Cli.Input
{
    internal static class ConsoleInput
    {
        // invariantculture for diferent styles for numbers, like 20.5 or 20,5
        public static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write($"{prompt} ({min} - {max}): ");
                string? UserInp = Console.ReadLine();

                if (int.TryParse(UserInp, CultureInfo.InvariantCulture, out int value)
                    && value >= min && value <= max)
                {
                    return value;
                }

                Console.WriteLine($"  Please enter a whole number between {min} and {max}.");
            }
        }
        public static double ReadDouble(string prompt, double min, double max)
        {
            while (true)
            {
                Console.Write($"{prompt} ({min} - {max}): ");
                string? UserInp = Console.ReadLine();

                if(double.TryParse(UserInp,CultureInfo.InvariantCulture, out double value) 
                    && value >= min && value <= max)
                {
                    return value;
                }
                Console.WriteLine($" Please enter a number between {min} and {max}");
            }
        }
        public static decimal ReadDecimal(string prompt, decimal min)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                string? UserInp = Console.ReadLine();

                if (decimal.TryParse(UserInp,CultureInfo.InvariantCulture, out decimal value) 
                    && value >= min )
                {
                    return value;
                }

                Console.WriteLine($" Please Enter a number bigger than ({min}): ");
            }
        }
        public static string? OptionalText(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt}: ");
                string? UserInp = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(UserInp)) return null;
                return UserInp.Trim();
            }
        }
        public static int ReadMonth(string prompt)
        {
            string[] fullNames = CultureInfo.InvariantCulture.DateTimeFormat.MonthNames; // for full names 
            string[] shortNames = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames; // for short names

            while (true)
            {
                Console.Write($"{prompt}: ");
                string UserInp = Console.ReadLine()?.Trim() ?? "";

                if (UserInp.Length > 0)
                {
                    if (int.TryParse(UserInp, out int number) && number is >= 1 and <= 12)
                    {
                        return number;
                    }
                    for (int i = 0; i < 12; i++)
                    {
                        if (UserInp.Equals(fullNames[i], StringComparison.OrdinalIgnoreCase) || UserInp.Equals(shortNames[i], StringComparison.OrdinalIgnoreCase))
                        {
                            // ordinal for comparison by num code like A 65 or a 97 etc etc etc
                            return i + 1;
                        }
                        
                    }
                }
                Console.WriteLine("Please enter a month number/name.");
            }
        }
    }
}
