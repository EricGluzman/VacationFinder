using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VacationFinder.Infrastructure.OpenMeteo
{
    internal record OpenMeteoResponse
    {
        [JsonPropertyName("daily")]
        public OpenMeteoDaily? Daily { get; init; }
    }

    internal record OpenMeteoDaily
    {
        [JsonPropertyName("time")]
        public DateOnly[] Time { get; init; } = [];

        [JsonPropertyName("temperature_2m_mean")]
        public double?[] TemperatureMean { get; init; } = [];

        [JsonPropertyName("precipitation_sum")]
        public double?[] PrecipitationSum { get; init; } = [];
    }
}
