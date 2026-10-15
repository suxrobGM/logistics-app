using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

/// <summary>
/// Marks an empty import container as street-turned to an export booking instead of being
/// returned to the line, which stops its per-diem clock.
/// </summary>
[RequiresFeature(TenantFeature.IntermodalContainers)]
public class StreetTurnContainerCommand : ICommand<Result>
{
    public Guid Id { get; set; }
    public required string ExportBookingReference { get; set; }
}
