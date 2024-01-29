using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Org.Quickstart.API.Models;

public record AirlineCreateRequestCommand
{
    [Required]
    [JsonPropertyName("callsign")]
    public string Callsign { get; set; } = string.Empty;
    
    [Required]
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
    
    [JsonPropertyName("iata")]
    public string Iata { get; set; } = string.Empty;
    
    [JsonPropertyName("icao")]
    public string Icao { get; set; } = string.Empty;
    
    [Required]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    public Airline GetAirline()
    {
        return new Airline()
        {
            Callsign = this.Callsign,
            Country = this.Country,
            Iata = this.Iata,
            Icao = this.Icao,
            Name = this.Name
        };
    }
}