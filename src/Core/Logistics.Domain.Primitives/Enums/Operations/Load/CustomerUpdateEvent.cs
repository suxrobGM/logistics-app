namespace Logistics.Domain.Primitives.Enums;

public enum CustomerUpdateEvent
{
    Dispatched,
    PickedUp,
    Delivered,
    Late,

    /// <summary>Drafted by the dispatch agent, approved by a dispatcher.</summary>
    DelayNotice
}
