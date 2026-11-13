namespace Logistics.Domain.Primitives.Enums;

public enum LoadStatus
{
    Draft, // created/quoted/booked but not dispatched yet
    Dispatched,
    PickedUp,
    Delivered,
    Cancelled // aborted at any stage before Delivered
}
