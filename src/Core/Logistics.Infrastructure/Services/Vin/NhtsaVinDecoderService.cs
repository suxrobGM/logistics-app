using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Logistics.Application.Services;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Services;

/// <summary>
///     VIN decoder using the free NHTSA (National Highway Traffic Safety Administration) API.
///     API Documentation: https://vpic.nhtsa.dot.gov/api/
/// </summary>
public class NhtsaVinDecoderService(HttpClient httpClient, ILogger<NhtsaVinDecoderService> logger)
    : IVinDecoderService
{
    private const string BaseUrl = "https://vpic.nhtsa.dot.gov/api/vehicles";

    public async Task<VehicleInfoDto?> DecodeVinAsync(string vin, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(vin) || vin.Length != 17)
        {
            logger.LogWarning("Invalid VIN provided: {Vin}", vin);
            return null;
        }

        try
        {
            var url = $"{BaseUrl}/DecodeVinValues/{vin}?format=json";
            var response = await httpClient.GetFromJsonAsync<NhtsaResponse>(url, ct);

            if (response?.Results == null || response.Results.Count == 0)
            {
                logger.LogWarning("No results returned from NHTSA for VIN: {Vin}", vin);
                return null;
            }

            var result = response.Results[0];

            // Check for error codes - 0 means successful decode
            if (!string.IsNullOrEmpty(result.ErrorCode) && result.ErrorCode != "0")
            {
                logger.LogWarning("NHTSA returned error for VIN {Vin}: {ErrorText}", vin, result.ErrorText);
            }

            int? year = null;
            if (int.TryParse(result.ModelYear, out var parsedYear))
            {
                year = parsedYear;
            }

            return new VehicleInfoDto(
                vin.ToUpperInvariant(),
                year,
                string.IsNullOrEmpty(result.Make) ? null : result.Make,
                string.IsNullOrEmpty(result.Model) ? null : result.Model,
                string.IsNullOrEmpty(result.BodyClass) ? null : result.BodyClass,
                string.IsNullOrEmpty(result.VehicleType) ? null : result.VehicleType,
                string.IsNullOrEmpty(result.DriveType) ? null : result.DriveType,
                string.IsNullOrEmpty(result.FuelTypePrimary) ? null : result.FuelTypePrimary,
                BuildEngineInfo(result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to decode VIN: {Vin}", vin);
            return null;
        }
    }

    private static string? BuildEngineInfo(NhtsaResult result)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(result.DisplacementL))
        {
            parts.Add($"{result.DisplacementL}L");
        }

        if (!string.IsNullOrEmpty(result.EngineCylinders))
        {
            parts.Add($"{result.EngineCylinders}cyl");
        }

        if (!string.IsNullOrEmpty(result.EngineHP))
        {
            parts.Add($"{result.EngineHP}hp");
        }

        return parts.Count > 0 ? string.Join(" ", parts) : null;
    }
}

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
