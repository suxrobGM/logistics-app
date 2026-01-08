using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class CreateTripCommand : IAppRequest
{
    public string Name { get; set; } = null!;
    public Guid TruckId { get; set; }
    public IEnumerable<CreateTripLoadCommand>? NewLoads { get; set; }
    public IEnumerable<Guid>? AttachedLoadIds { get; set; }
    public IEnumerable<TripStopDto>? OptimizedStops { get; set; }

    /// <summary>
    ///     Total distance from the route optimizer (in kilometers).
    /// </summary>
    public double? TotalDistance { get; set; }
}
