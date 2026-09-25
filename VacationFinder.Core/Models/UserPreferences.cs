namespace VacationFinder.Core.Models
{
    public record UserPreferences {
        public double MinTemp { get; }
        public double MaxTemp { get; }
        public int Month { get; }
        public decimal Budget { get;}
        public int Days { get; }
        public string? Continent { get; }
        public string? Country { get;}
        public UserPreferences(double minTemp, double maxTemp, int month, decimal budget, int days, string? continent = null, string? country = null)
        {
            if (minTemp > maxTemp) throw new ArgumentException("Min Temp must be lower than the Max Temp");
            if (minTemp < -100) throw new ArgumentOutOfRangeException(nameof(minTemp), "Min Temperature should be higher than -100.");
            if (maxTemp > 100 ) throw new ArgumentOutOfRangeException(nameof(maxTemp), "Max Temperature should be lower than 100.");
            if (month < 1 || month > 12) throw new ArgumentException("Month value should be in range of 1-12.",nameof(month));
            if (budget <= 0) throw new ArgumentException("Budget must be higher than 0.",nameof(budget));
            if(days <= 0) throw new ArgumentException("Days value must be higher than 0.",nameof(days));
            this.MinTemp = minTemp;
            this.MaxTemp = maxTemp;
            this.Month = month;
            this.Budget = budget;
            this.Days = days;
            this.Continent = continent;
            this.Country = country;
        }
    }
}
