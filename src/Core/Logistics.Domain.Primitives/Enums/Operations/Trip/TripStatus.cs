namespace Logistics.Domain.Primitives.Enums;

public enum TripStatus
{
    Draft, // loads assigned, not yet dispatched
    Dispatched, // driver left origin yard
    InTransit, // at least one load picked up, not all delivered
    Completed, // all loads delivered
    Cancelled
}
