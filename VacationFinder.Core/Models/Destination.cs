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
        public Destination(string cityName, string country,string countryCode, string continent, double latitude, double longitude, decimal flightCostUsd, decimal dailyCostUsd)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cityName);
            ArgumentException.ThrowIfNullOrWhiteSpace(country);
            ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(continent);

            string cleanCode = countryCode.Trim().ToUpperInvariant();
            if (cleanCode.Length != 2) throw new ArgumentException("Country code must be 2 letters like ES.", nameof(countryCode));

            if (latitude < -90 || latitude > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
            }

            if (longitude < -180 || longitude > 180)
            {
                throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");
            }

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(flightCostUsd);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dailyCostUsd);

            this.Country = country.Trim();
            this.Continent = continent.Trim();            
            // if user choose to enter both, country and continent then the pref is on country.
            // if both clean (null) then its gonna be around the world options, not tight on country or continent.

            this.CityName = cityName;
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.CountryCode = cleanCode;
        }
        public decimal TotalCostUsd(int days) => FlightCostUsd + DailyCostUsd * days;
    }
}
