using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

[RequiresFeature(TenantFeature.Safety)]
public class DecodeVinCommand : IAppRequest<Result<VehicleInfoDto>>
{
    public required string Vin { get; set; }
}
