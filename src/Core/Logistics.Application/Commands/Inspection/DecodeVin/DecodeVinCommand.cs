using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class DecodeVinCommand : IAppRequest<Result<VehicleInfoDto>>
{
    public required string Vin { get; set; }
}
