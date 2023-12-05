using System.Text.Json.Serialization;

namespace Org.Quickstart.API.Models;

public record Geo
{
    [JsonPropertyName("alt")]
    public double Alt { get; set; }

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }
}