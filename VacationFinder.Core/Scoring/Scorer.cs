namespace VacationFinder.Core.Scoring
{
    public static class Scorer
    {
        public static double TemperatureScore(double destavgtemp,double min,double max)
        {
            const double MaxRange = 6.0; // the max dif of the temp. after that the score is 0. avg: 27 min: 11 max: 20 => score 0;
            if (destavgtemp >= min && destavgtemp <= max) return 1.0;

            double overflow = destavgtemp > max ? destavgtemp - max : min - destavgtemp;
            return Math.Max(0.0, 1.0 - overflow / MaxRange);
        }
        public static double PriceScore(decimal totalCost, decimal budget)
        {
            if (budget <= 0) throw new ArgumentException("Budget must be higher than 0.", nameof(budget));
            if (totalCost <= 0) throw new ArgumentException("Total Cost must be higher than 0.", nameof(totalCost));
            if (totalCost > budget) return 0;
            // calculates the ratio of total cost and the budget, and then gives the final score by removing the pow of 2 from 1.0
            // when the total cost is lower then the final score is higher.
            double ratio = (double)(totalCost / budget);
            return (1.0 - Math.Pow(ratio, 2));
        }
        public static double FinalScore(double tempScore, double priceScore,double tempWeight,double priceWeight)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(tempWeight);
            ArgumentOutOfRangeException.ThrowIfNegative(priceWeight);

            if (tempWeight == 0 && priceWeight == 0)
            {
                tempWeight = 0.4;
                priceWeight = 0.6; // in my opinion the price is the factor of choice, so a higher weight for that one.
            }
            double totalWeight = tempWeight + priceWeight;
            // Normalize weights so their sum is exactly 1.0 with the same ratio.
            double normalizedTempWeight = tempWeight / totalWeight;
            double normalizedPriceWeight = priceWeight / totalWeight;
            return Math.Max(0,(tempScore * normalizedTempWeight) + (priceScore * normalizedPriceWeight));
        }
    }
}
