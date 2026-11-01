using VacationFinder.Core.Models;
namespace VacationFinder.Core.Interfaces;
public interface IClimateProvider
{
    Task<ClimateInfo> GetClimateAsync(Destination destination, int month, CancellationToken ct = default);
} // I dont quite understand what that does. Claude did this part. I tried to understand it but failed.
