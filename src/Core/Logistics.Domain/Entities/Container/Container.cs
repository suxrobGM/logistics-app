using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents an intermodal container that can move across multiple loads
/// over its lifecycle (empty pickup → loaded → in transit → delivered → returned).
/// </summary>
public partial class Container : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// ISO 6346 container number - 4 letters (owner + category) + 7 digits.
    /// </summary>
    public required string Number { get; set; }

    public required ContainerIsoType IsoType { get; set; }

    public string? SealNumber { get; set; }
    public string? BookingReference { get; set; }
    public string? BillOfLadingNumber { get; set; }

    /// <summary>
    /// True when the container is loaded with cargo, false when empty.
    /// </summary>
    public bool IsLaden { get; set; }

    /// <summary>
    /// Gross weight in kilograms or pounds depending on the tenant's unit setting.
    /// </summary>
    public decimal GrossWeight { get; set; }

    public ContainerStatus Status { get; private set; } = ContainerStatus.Empty;

    public Guid? CurrentTerminalId { get; set; }
    public virtual Terminal? CurrentTerminal { get; set; }

    public string? Notes { get; set; }

    public DateTime? LoadedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ReturnedAt { get; private set; }
}
