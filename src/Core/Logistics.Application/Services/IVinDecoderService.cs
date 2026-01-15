using Logistics.Shared.Models;

namespace Logistics.Application.Services;

/// <summary>
///    Service for decoding VINs (Vehicle Identification Numbers).
/// </summary>
public interface IVinDecoderService
{
    Task<VehicleInfoDto?> DecodeVinAsync(string vin, CancellationToken ct = default);
}
