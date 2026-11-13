using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>Stored as JSON on the load row.</summary>
public class CustomerUpdate
{
    public CustomerUpdateEvent Event { get; set; }
    public CustomerUpdateOutcome Outcome { get; set; }
    public DateTime At { get; set; }
    public int RecipientCount { get; set; }

    public string? Reason { get; set; }
}
