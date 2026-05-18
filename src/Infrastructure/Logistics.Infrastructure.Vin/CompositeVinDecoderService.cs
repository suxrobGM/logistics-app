using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Vin;

namespace Logistics.Infrastructure.Vin;

/// <summary>
///     Orchestrates a chain of VIN decoders: WMI prefix lookup (covers EU / non-US VINs)
///     plus NHTSA full-VIN decode (rich US detail). Results are merged so the response
///     surfaces the best available data and a <c>Source</c> tag indicating which providers
///     contributed.
/// </summary>
internal sealed class CompositeVinDecoderService(
    WmiPrefixDecoder wmi,
    NhtsaVinDecoderService nhtsa,
    ILogger<CompositeVinDecoderService> logger)
    : IVinDecoderService
{
    public async Task<VehicleInfoDto?> DecodeVinAsync(string vin, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(vin) || vin.Length != 17)
        {
            logger.LogWarning("Invalid VIN provided to composite decoder: {Vin}", vin);
            return null;
        }

        var wmiResult = await wmi.DecodeVinAsync(vin, ct);
        var nhtsaResult = await nhtsa.DecodeVinAsync(vin, ct);

        return (wmiResult, nhtsaResult) switch
        {
            (null, null) => null,
            (null, _) => nhtsaResult,
            (_, null) => wmiResult,
            _ => Merge(wmiResult, nhtsaResult)
        };
    }

    private static VehicleInfoDto Merge(VehicleInfoDto wmi, VehicleInfoDto nhtsa)
    {
        return nhtsa with
        {
            Make = nhtsa.Make ?? wmi.Make,
            VehicleType = nhtsa.VehicleType ?? wmi.VehicleType,
            CountryOfManufacture = nhtsa.CountryOfManufacture ?? wmi.CountryOfManufacture,
            Source = "wmi+nhtsa"
        };
    }
}
