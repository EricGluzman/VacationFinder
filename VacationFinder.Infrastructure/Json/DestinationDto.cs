using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacationFinder.Infrastructure.Json;

internal record DestinationDto
{
    public string? City { get; init; }
    public string? Country { get; init; }
    public string? CountryCode { get; init; }
    public string? Continent { get; init; }
    public double? Lat { get; init; }
    public double? Lng { get; init; }
}