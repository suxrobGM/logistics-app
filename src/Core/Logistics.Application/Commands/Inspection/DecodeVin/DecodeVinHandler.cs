using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DecodeVinHandler(IVinDecoderService vinDecoder)
    : IAppRequestHandler<DecodeVinCommand, Result<VehicleInfoDto>>
{
    public async Task<Result<VehicleInfoDto>> Handle(DecodeVinCommand req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Vin))
        {
            return Result<VehicleInfoDto>.Fail("VIN is required");
        }

        // Clean up VIN
        var vin = req.Vin.Trim().ToUpperInvariant();

        if (vin.Length != 17)
        {
            return Result<VehicleInfoDto>.Fail("VIN must be exactly 17 characters");
        }

        var vehicleInfo = await vinDecoder.DecodeVinAsync(vin, ct);

        if (vehicleInfo is null)
        {
            return Result<VehicleInfoDto>.Fail("Unable to decode VIN. Please verify the VIN is correct.");
        }

        return Result<VehicleInfoDto>.Ok(vehicleInfo);
    }
}
