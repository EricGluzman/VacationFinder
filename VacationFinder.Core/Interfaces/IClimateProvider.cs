using VacationFinder.Core.Models;
namespace VacationFinder.Core.Interfaces;
public interface IClimateProvider
{
    Task<ClimateInfo> GetClimateAsync(Destination destination, int month, CancellationToken ct = default);
}
