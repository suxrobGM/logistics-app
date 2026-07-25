using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents an intermodal container that can move across multiple loads
/// over its lifecycle (empty pickup → loaded → in transit → delivered → returned).
/// </summary>
public partial class Container : AuditableEntity, ITenantEntity
{
    private string number = "";

    /// <summary>
    /// ISO 6346 container number - 4 letters (owner + category) + 7 digits.
    /// <para>
    /// Canonicalised on write by <see cref="NormalizeNumber"/>, so the unique index cannot be
    /// defeated by case and lookups can compare exactly instead of wrapping the column in
    /// <c>upper()</c> (which no index can serve).
    /// </para>
    /// </summary>
    public required string Number
    {
        get => number;
        set => number = NormalizeNumber(value);
    }

    /// <summary>
    /// The canonical form of a container number. Callers that query by number must normalise the
    /// search term through this, since the stored value is always canonical.
    /// </summary>
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
