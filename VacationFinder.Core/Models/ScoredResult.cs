namespace VacationFinder.Core.Models
{
    public record ScoredResult(Destination Destination, ClimateInfo Climate, decimal TotalCost, double FinalScore);
}
