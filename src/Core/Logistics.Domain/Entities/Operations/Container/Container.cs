using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public partial class Container : AuditableEntity, ITenantEntity
{
    private string _number = "";

    /// <summary>Container number, stored trimmed and upper-case so the unique index and exact lookups are case-insensitive.</summary>
    public required string Number
    {
        get => _number;
        set => _number = NormalizeNumber(value);
    }

    /// <summary>Trims and upper-cases a container number. Queries by number apply it to the search term, since stored numbers are normalized.</summary>
    public static string NormalizeNumber(string? value) =>
        value?.Trim().ToUpperInvariant() ?? "";

    public required ContainerIsoType IsoType { get; set; }

    public string? SealNumber { get; set; }
    public string? BookingReference { get; set; }
    public string? BillOfLadingNumber { get; set; }

    /// <summary>
    /// True when the container is loaded with cargo, false when empty.
    /// </summary>
    public bool IsLaden { get; set; }

    /// <summary>Gross weight in kilograms.</summary>
    public decimal GrossWeightKg { get; set; }

    public ContainerStatus Status { get; private set; } = ContainerStatus.Empty;

    public Guid? CurrentTerminalId { get; set; }
    public virtual Terminal? CurrentTerminal { get; set; }

    public string? Notes { get; set; }

    public DateTime? LoadedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ReturnedAt { get; private set; }
}
