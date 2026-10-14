using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Compliance.Eld.Commands;

[RequiresFeature(TenantFeature.Eld)]
public class DeleteEldVehicleMappingCommand : ICommand
{
    public Guid MappingId { get; set; }
}
