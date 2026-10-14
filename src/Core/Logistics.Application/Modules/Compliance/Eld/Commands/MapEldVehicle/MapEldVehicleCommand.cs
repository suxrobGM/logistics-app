using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Compliance.Eld.Commands;

[RequiresFeature(TenantFeature.Eld)]
public class MapEldVehicleCommand : ICommand
{
    public Guid TruckId { get; set; }
    public EldProviderType ProviderType { get; set; }
    public required string ExternalVehicleId { get; set; }
    public string? ExternalVehicleName { get; set; }
}
