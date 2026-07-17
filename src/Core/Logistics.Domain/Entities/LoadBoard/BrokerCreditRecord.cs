using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Cached broker credit lookup keyed by MC number. Doubles as an audit trail of
/// what credit data was known at booking time in case of a payment dispute.
/// </summary>
public class BrokerCreditRecord : AuditableEntity, ITenantEntity
{
    public required string McNumber { get; set; }

    /// <summary>
    /// Credit score normalized to 0-100. Null when the source has no score (e.g. FMCSA authority-only).
    /// </summary>
    public int? CreditScore { get; set; }

    /// <summary>
    /// Average days the broker takes to pay carriers.
    /// </summary>
    public int? DaysToPay { get; set; }

    /// <summary>
    /// Whether the broker's operating authority is active per FMCSA. Null when not checked.
    /// </summary>
    public bool? AuthorityActive { get; set; }

    public required BrokerCreditSource Source { get; set; }

    public required DateTime CheckedAt { get; set; }
}
