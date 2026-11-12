using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// One customer update email for a load: sent, skipped (with why) or failed. Stored as JSON on the load row.
/// </summary>
public class CustomerUpdate
{
    public CustomerUpdateEvent Event { get; set; }
    public CustomerUpdateOutcome Outcome { get; set; }
    public DateTime At { get; set; }
    public int RecipientCount { get; set; }

    /// <summary>
    /// Why the update was skipped or failed; null when it was sent.
    /// </summary>
    public string? Reason { get; set; }
}
