using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using VacationFinder.Core.Interfaces;
using VacationFinder.Core.Models;

namespace VacationFinder.Infrastructure.Json;
/// this file is made with the help of claude!
/// this file is made with the help of claude!

/// <summary>
/// Loads destinations from a JSON file. Broken entries are skipped with a warning
/// instead of stopping the whole load.
/// </summary>
public class JsonDestinationSource : IDestinationSource
{
    // Created once and reused: .NET caches work per options instance, so a new one per call is slow.
    // Web defaults = property names are matched case-insensitively ("city" -> City).
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly string _filePath;

    public JsonDestinationSource(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
    }

    public async Task<IReadOnlyList<Destination>> GetDestinationsAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException($"Destinations file not found: {_filePath}", _filePath);

        DestinationDto[]? dtos;
        await using (FileStream stream = File.OpenRead(_filePath))
        {
            dtos = await JsonSerializer.DeserializeAsync<DestinationDto[]>(stream, JsonOptions, ct);
        }

        if (dtos is null || dtos.Length == 0)
            throw new InvalidOperationException($"Destinations file is empty: {_filePath}");

        var result = new List<Destination>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // to catch duplicates

        for (int i = 0; i < dtos.Length; i++)
        {
            Destination? destination = TryConvert(dtos[i]);

            if (destination is null)
            {
                Console.WriteLine($"Warning: skipped entry #{i + 1} ({dtos[i].City ?? "unknown city"}): missing or invalid data.");
                continue;
            }
            string key = $"{destination.CityName}|{destination.CountryCode}";
            if (!seen.Add(key))
            {
                Console.WriteLine($"Warning: skipped entry #{i + 1} ({destination.CityName}): duplicate.");
                continue;
            }

            result.Add(destination);
        }

        return result;
    }
    // Converts one DTO into a Destination, or returns null if the data is bad.
    private static Destination? TryConvert(DestinationDto dto)
    {
        // Missing coordinates must be caught here; otherwise they'd silently become 0, 0.
        if (dto.Lat is null || dto.Lng is null)
            return null;
        try
        {
            // Destination validates everything else (empty names, code length, coordinate ranges).
            return new Destination(
                dto.City!,
                dto.Country!,
                dto.CountryCode!,
                dto.Continent!,
                dto.Lat.Value,
                dto.Lng.Value);
        }
        catch (ArgumentException)
        {
            // Covers ArgumentNullException and ArgumentOutOfRangeException too (they inherit from it).
            return null;
        }
    }
}