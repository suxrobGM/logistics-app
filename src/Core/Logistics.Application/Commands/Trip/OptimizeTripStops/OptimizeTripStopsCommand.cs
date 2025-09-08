using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public sealed class OptimizeTripStopsCommand : IAppRequest<Result<OptimizedTripStopsDto>>
{
    /// <summary>
    ///     Which strategy to use for route optimization.
    /// </summary>
    // public TripRoutingStrategy Strategy { get; set; }

    /// <summary>
    ///     Specify the capacity of the car hauler truck which is used for route optimization.
    /// </summary>
    public int MaxVehicles { get; set; }

    /// <summary>
    ///     Specify stops that do not exist in the database for a new trip.
    /// </summary>
    public IEnumerable<TripStopDto> Stops { get; set; } = null!;
}
