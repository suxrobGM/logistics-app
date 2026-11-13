using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// A deduction taken from every settlement of a driver - the usual owner-operator lease charges
/// (truck lease, insurance, ELD fee) and the escrow contribution.
/// </summary>
public class RecurringDeduction : Entity, ITenantEntity
{
    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    public required PayDeductionType Type { get; set; }
    public required string Description { get; set; }

    /// <summary>Amount per settlement in the tenant currency, positive.</summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Escrow only: contributions stop once the escrow balance reaches this amount. Null keeps
    /// contributing every settlement.
    /// </summary>
    public decimal? EscrowTarget { get; set; }
}
