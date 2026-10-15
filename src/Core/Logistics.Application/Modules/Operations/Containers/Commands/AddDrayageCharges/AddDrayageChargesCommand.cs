using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

/// <summary>
/// Writes the load's container demurrage, per-diem and chassis charges as of now as load
/// accessorials and syncs the invoice. Re-running replaces the ones it wrote last time.
/// </summary>
[RequiresFeature(TenantFeature.IntermodalContainers)]
public class AddDrayageChargesCommand : ICommand<Result>
{
    public Guid LoadId { get; set; }
}
