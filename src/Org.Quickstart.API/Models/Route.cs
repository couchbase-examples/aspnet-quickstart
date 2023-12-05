using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Org.Quickstart.API.Models;


public record Route
{
    [JsonPropertyName("airline")]
    public string Airline { get; set; } = string.Empty;

    [JsonPropertyName("airlineid")]
    public string AirlineId { get; set; } = string.Empty;

    [JsonPropertyName("destinationairport")]
    public string DestinationAirport { get; set; } = string.Empty;

    [JsonPropertyName("distance")]
    public double Distance { get; set; }

    [JsonPropertyName("equipment")]
    public string Equipment { get; set; } = string.Empty;

    [JsonPropertyName("schedule")]
    public List<Schedule> Schedule { get; set; }

    [JsonPropertyName("sourceairport")]
    public string SourceAirport { get; set; } = string.Empty;
    
    [JsonPropertyName("stops")]
    public int Stops { get; set; }
}