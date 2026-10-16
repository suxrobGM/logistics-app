using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class Trailer : AuditableEntity, ITenantEntity
{
    public required string Number { get; set; }

    public required TrailerType Type { get; set; }

    public TrailerStatus Status { get; set; } = TrailerStatus.Active;

    public string? Vin { get; set; }

    public string? LicensePlate { get; set; }

    /// <summary>
    ///     State or region where the plate is registered (e.g., "TX", "ON").
    /// </summary>
    public string? LicensePlateState { get; set; }

    public int? Year { get; set; }

    public string? Make { get; set; }

    public string? Model { get; set; }

    /// <summary>
    ///     Overall length, always stored in metres; clients convert to feet for US-unit tenants.
    /// </summary>
    public decimal? LengthMeters { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    ///     Only an Active trailer can be hooked to a truck or pulled on a trip.
    /// </summary>
    /// <exception cref="InvalidOperationException">The trailer is not Active.</exception>
    public void EnsureCanBeHooked()
    {
        if (Status != TrailerStatus.Active)
        {
            throw new InvalidOperationException(
                $"Trailer '{Number}' is {Status.GetDescription()} and cannot be hooked");
        }
    }
}
