namespace Logistics.Shared.Models;

public record UpdateLoadProximityCommand
{
    public Guid? LoadId { get; set; }

    /// <summary>
    /// True when the truck is inside the geofence of the next confirmation checkpoint.
    /// </summary>
    public bool IsInProximity { get; set; }
}
