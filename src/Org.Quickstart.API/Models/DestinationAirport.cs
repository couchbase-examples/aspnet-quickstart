using System.Text.Json.Serialization;

namespace Org.Quickstart.API.Models;

public record DestinationAirport
{
    [JsonPropertyName("destinationairport")]
    public string Destinationairport { get; set; } = string.Empty;
}