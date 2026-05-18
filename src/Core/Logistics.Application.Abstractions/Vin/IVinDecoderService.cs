using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Vin;

namespace Logistics.Application.Abstractions.Vin;

/// <summary>
///    Service for decoding VINs (Vehicle Identification Numbers).
/// </summary>
public interface IVinDecoderService
{
    Task<VehicleInfoDto?> DecodeVinAsync(string vin, CancellationToken ct = default);
}
