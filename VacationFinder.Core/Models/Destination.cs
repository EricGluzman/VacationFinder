namespace VacationFinder.Core.Models
{
    public record Destination
    {
        public string CityName { get; }
        public string Country { get; }
        public string CountryCode { get; } 
        public string Continent { get; }
        public double Latitude { get; }
        public double Longitude { get; }
        public decimal FlightCostUsd { get; }
        public decimal DailyCostUsd { get; }
        public Destination(string cityName, string country,string countryCode, string continent, double latitude, double longitude, decimal flightCostUsd,
    decimal dailyCostUsd)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cityName);
            ArgumentException.ThrowIfNullOrWhiteSpace(country);
            ArgumentException.ThrowIfNullOrWhiteSpace(continent);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(flightCostUsd);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dailyCostUsd);

            if (latitude < -90 || latitude > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
            }

            if (longitude < -180 || longitude > 180)
            {
                throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);
            string cleanCode = countryCode.Trim().ToUpperInvariant();
            if (cleanCode.Length != 2) throw new ArgumentException("Country code must be 2 letters, like ES.", nameof(countryCode));

            this.Country = country.Trim();
            this.Continent = continent.Trim();

            this.FlightCostUsd = flightCostUsd;
            this.DailyCostUsd = dailyCostUsd;
            this.CityName = cityName;
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.CountryCode = cleanCode;

        }
    }
}
