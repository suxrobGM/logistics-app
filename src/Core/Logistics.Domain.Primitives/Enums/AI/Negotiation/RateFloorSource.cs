namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Which rule supplied the floor a negotiation was opened with, most specific first.
/// </summary>
public enum RateFloorSource
{
    None,
    LaneExact,
    LaneOriginAny,
    LaneDestinationAny,
    TenantDefault
}
