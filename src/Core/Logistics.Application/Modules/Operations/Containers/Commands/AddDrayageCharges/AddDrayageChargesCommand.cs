using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

/// <summary>
/// Writes the load's container demurrage, per-diem and chassis charges onto the load's invoice as
/// of now. Re-running replaces the lines it wrote last time instead of adding a second set.
/// </summary>
[RequiresFeature(TenantFeature.IntermodalContainers)]
public class AddDrayageChargesCommand : ICommand<Result>
{
    public Guid LoadId { get; set; }
}
