using System.Net.Http.Json;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Vin;

namespace Logistics.Infrastructure.Vin;

/// <summary>
///     Full-VIN decoder using NHTSA's <c>DecodeVinValues</c> endpoint.
///     Reliable for US-market VINs; returns sparse / null fields for EU VINs.
///     API documentation: https://vpic.nhtsa.dot.gov/api/
/// </summary>
internal sealed class NhtsaVinDecoderService(HttpClient httpClient, ILogger<NhtsaVinDecoderService> logger)
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
            var response = await httpClient.GetFromJsonAsync<NhtsaVinValuesResponse>(url, ct);

            if (response?.Results == null || response.Results.Count == 0)
            {
                logger.LogWarning("No results returned from NHTSA for VIN: {Vin}", vin);
                return null;
            }

            var result = response.Results[0];

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
                BuildEngineInfo(result),
                CountryOfManufacture: null,
                Source: "nhtsa");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to decode VIN: {Vin}", vin);
            return null;
        }
    }

    private static string? BuildEngineInfo(NhtsaVinValuesResult result)
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
