using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class UpdateLoadProximityCommand : IAppRequest
{
    public Guid LoadId { get; set; }

    /// <summary>
    /// True when the truck is inside the geofence of the next confirmation checkpoint.
    /// The "next checkpoint" is implied by the load's current status:
    /// pickup if Dispatched, delivery if PickedUp.
    /// </summary>
    public bool IsInProximity { get; set; }
}
