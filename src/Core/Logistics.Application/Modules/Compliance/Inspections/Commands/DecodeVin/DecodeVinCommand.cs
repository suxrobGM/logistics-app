using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Inspections.Commands;

[RequiresFeature(TenantFeature.Safety)]
public class DecodeVinCommand : ICommand<Result<VehicleInfoDto>>
{
    public required string Vin { get; set; }
}
