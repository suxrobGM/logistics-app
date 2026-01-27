using System.Text.Json.Serialization;

namespace Logistics.Infrastructure.Documents.Vin;

internal class NhtsaResponse
{
    [JsonPropertyName("Count")]
    public int Count { get; set; }

    [JsonPropertyName("Results")]
    public List<NhtsaResult> Results { get; set; } = new();
}

internal class NhtsaResult
{
    [JsonPropertyName("ErrorCode")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("ErrorText")]
    public string? ErrorText { get; set; }

    [JsonPropertyName("Make")]
    public string? Make { get; set; }

    [JsonPropertyName("Model")]
    public string? Model { get; set; }

    [JsonPropertyName("ModelYear")]
    public string? ModelYear { get; set; }

    [JsonPropertyName("BodyClass")]
    public string? BodyClass { get; set; }

    [JsonPropertyName("VehicleType")]
    public string? VehicleType { get; set; }

    [JsonPropertyName("DriveType")]
    public string? DriveType { get; set; }

    [JsonPropertyName("FuelTypePrimary")]
    public string? FuelTypePrimary { get; set; }

    [JsonPropertyName("DisplacementL")]
    public string? DisplacementL { get; set; }

    [JsonPropertyName("EngineCylinders")]
    public string? EngineCylinders { get; set; }

    [JsonPropertyName("EngineHP")]
    public string? EngineHP { get; set; }
}
