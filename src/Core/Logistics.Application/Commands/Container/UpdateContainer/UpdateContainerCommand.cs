using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Updates non-status fields of a Container. Status transitions go through
/// the dedicated MarkContainer* commands; location-only moves use MoveContainerToTerminal.
/// </summary>
public class UpdateContainerCommand : IAppRequest<Result>
{
    public Guid Id { get; set; }
    public string? Number { get; set; }
    public ContainerIsoType? IsoType { get; set; }
    public string? SealNumber { get; set; }
    public string? BookingReference { get; set; }
    public string? BillOfLadingNumber { get; set; }
    public decimal? GrossWeight { get; set; }
    public string? Notes { get; set; }
}
