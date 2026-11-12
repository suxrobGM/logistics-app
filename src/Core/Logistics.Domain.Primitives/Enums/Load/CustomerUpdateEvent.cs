namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// What a customer update email told the customer about their shipment.
/// </summary>
public enum CustomerUpdateEvent
{
    Dispatched,
    PickedUp,
    Delivered,
    Late,

    /// <summary>
    /// A free-text delay notice the AI dispatch agent drafted and a dispatcher approved.
    /// </summary>
    DelayNotice
}
