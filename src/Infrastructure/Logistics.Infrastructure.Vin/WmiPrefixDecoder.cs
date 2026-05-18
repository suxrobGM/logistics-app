using System.Globalization;
using System.Net.Http.Json;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Vin;

namespace Logistics.Infrastructure.Vin;

/// <summary>
///     Decodes the 3-character World Manufacturer Identifier (WMI) prefix of a VIN
///     into manufacturer and country of origin. Currently calls NHTSA's <c>DecodeWMI</c>
///     endpoint, whose database is sourced from ISO 3779 and covers global manufacturers
///     (works for EU/JP/KR VINs that the full <c>DecodeVinValues</c> endpoint cannot decode).
/// </summary>
internal sealed class WmiPrefixDecoder(HttpClient httpClient, ILogger<WmiPrefixDecoder> logger)
    : IVinDecoderService
{
    private const string BaseUrl = "https://vpic.nhtsa.dot.gov/api/vehicles";

    public async Task<VehicleInfoDto?> DecodeVinAsync(string vin, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(vin) || vin.Length != 17)
        {
            logger.LogWarning("Invalid VIN provided to WMI decoder: {Vin}", vin);
            return null;
        }

        var prefix = vin.Substring(0, 3).ToUpperInvariant();

        try
        {
            var url = $"{BaseUrl}/DecodeWMI/{prefix}?format=json";
            var response = await httpClient.GetFromJsonAsync<NhtsaWmiResponse>(url, ct);

            if (response?.Results == null || response.Results.Count == 0)
            {
                logger.LogInformation("No WMI match for prefix: {Prefix}", prefix);
                return null;
            }

            var result = response.Results[0];

            var make = FirstNonEmpty(result.CommonName, result.ManufacturerName, result.Name);
            var country = TitleCase(result.CountryName);
            var vehicleType = string.IsNullOrEmpty(result.VehicleType) ? null : result.VehicleType;

            if (make is null && country is null)
            {
                return null;
            }

            return new VehicleInfoDto(
                vin.ToUpperInvariant(),
                Year: null,
                make,
                Model: null,
                BodyClass: null,
                vehicleType,
                DriveType: null,
                FuelType: null,
                EngineInfo: null,
                CountryOfManufacture: country,
                Source: "wmi");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to decode WMI prefix {Prefix}", prefix);
            return null;
        }
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static string? TitleCase(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.ToLowerInvariant());
    }
}
