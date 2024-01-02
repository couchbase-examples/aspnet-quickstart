using System.Text.Json.Serialization;

namespace Org.Quickstart.API.Models;

public record Schedule
{
    [JsonPropertyName("day")]
    public int Day { get; set; }
    
    [JsonPropertyName("flight")]
    public string Flight { get; set; } = string.Empty;
    
    [JsonPropertyName("utc")]
    public string Utc { get; set; } = string.Empty;
}