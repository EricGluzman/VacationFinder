namespace VacationFinder.Core.Models
{
    public record Destination
    {
        public string CityName { get; }
        public string Country { get; }
        public string Continent { get; }
        public double Latitude { get; }
        public double Longitude { get; }
        public Destination(string cityName, string country, string continent, double latitude, double longitude)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cityName);
            ArgumentException.ThrowIfNullOrWhiteSpace(country);
            ArgumentException.ThrowIfNullOrWhiteSpace(continent);
            if (latitude < -90 || latitude > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
            }
            if (longitude < -180 || longitude > 180)
            {
                throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");
            }

            string? cleanCountry = string.IsNullOrWhiteSpace(country) ? null : country.Trim();
            string? cleanContinent = string.IsNullOrWhiteSpace(continent) ? null : continent.Trim();
            this.Country = cleanCountry;
            this.Continent = cleanCountry is null ? cleanContinent : null; 
            // if user choose to enter both, country and continent then the pref is on country.
            // if both clean (null) then its gonna be around the world options, not tight on country or continent.

            this.CityName = cityName;
            this.Latitude = latitude;
            this.Longitude = longitude;
        }
    }
}
