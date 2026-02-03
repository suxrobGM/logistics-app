namespace Logistics.Domain.Primitives.Enums;

/// <summary>
///     Represents the strategies for determining the route of a trip to logistics operations.
/// </summary>
public enum TripRoutingStrategy
{
    /// <summary>
    ///     Pick up the first load (vehicle), then drop it off at the next load (vehicle).
    ///     Then pick up the next load (vehicle), and so on.
    /// </summary>
    Linear,

    /// <summary>
    ///     Pick up all loads (vehicles) one by one and then drop them off by order of arrival.
    ///     Heuristic algorithm: cluster pickups then drops.
    /// </summary>
    PickupsFirst,

    /// <summary>
    ///     Using PDPTW (Pickup and Delivery Problem with Time Windows) algorithm.
    ///     It will try to find the best route for the trip using Mapbox Matrix API and Google OR-Tools.
    /// </summary>
    Optimized
}
