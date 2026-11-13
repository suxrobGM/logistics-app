using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

/// <summary>Edits a container's descriptive fields. Status changes use the MarkContainer* commands and terminal moves use MoveContainerToTerminal.</summary>
[RequiresFeature(TenantFeature.IntermodalContainers)]
public class UpdateContainerCommand : ICommand<Result>
{
    public Guid Id { get; set; }
    public string? Number { get; set; }
    public ContainerIsoType? IsoType { get; set; }
    public string? SealNumber { get; set; }
    public string? BookingReference { get; set; }
    public string? BillOfLadingNumber { get; set; }
    public decimal? GrossWeightKg { get; set; }
    public string? Notes { get; set; }
}
