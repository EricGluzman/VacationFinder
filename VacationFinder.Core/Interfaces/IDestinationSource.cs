using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacationFinder.Core.Models;

namespace VacationFinder.Core.Interfaces;

public interface IDestinationSource
{
    Task<IReadOnlyList<Destination>> GetDestinationsAsync(CancellationToken ct = default);
}