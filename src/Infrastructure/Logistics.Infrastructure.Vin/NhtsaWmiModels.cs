using System.Text.Json.Serialization;

namespace Logistics.Infrastructure.Vin;

internal class NhtsaWmiResponse
{
    [JsonPropertyName("Count")]
    public int Count { get; set; }

    [JsonPropertyName("Results")]
    public List<NhtsaWmiResult> Results { get; set; } = new();
}

internal class NhtsaWmiResult
{
    [JsonPropertyName("ManufacturerName")]
    public string? ManufacturerName { get; set; }

    [JsonPropertyName("CommonName")]
    public string? CommonName { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("CountryName")]
    public string? CountryName { get; set; }

    [JsonPropertyName("VehicleType")]
    public string? VehicleType { get; set; }
}
