using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public partial class Load
{
    /// <summary>Oldest first.</summary>
    public List<CustomerUpdate> CustomerUpdates { get; private set; } = [];

    public bool WasCustomerUpdateSent(CustomerUpdateEvent updateEvent)
    {
        return CustomerUpdates.Any(u => u.Event == updateEvent && u.Outcome == CustomerUpdateOutcome.Sent);
    }

    public void RecordCustomerUpdate(
        CustomerUpdateEvent updateEvent,
        CustomerUpdateOutcome outcome,
        int recipientCount,
        string? reason = null)
    {
        CustomerUpdates =
        [
            .. CustomerUpdates,
            new CustomerUpdate
            {
                Event = updateEvent,
                Outcome = outcome,
                At = DateTime.UtcNow,
                RecipientCount = recipientCount,
                Reason = reason
            }
        ];
    }
}
