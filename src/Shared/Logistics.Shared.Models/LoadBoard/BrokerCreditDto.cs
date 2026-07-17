using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record BrokerCreditDto
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

    public BrokerCreditSource Source { get; set; }

    public DateTime CheckedAt { get; set; }
}
