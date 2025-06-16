using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class TripLoad : Entity, ITenantEntity
{
    public Guid TripId { get; set; }
    public virtual Trip Trip { get; set; } = null!;

    public Guid LoadId { get; set; }
    public virtual Load Load { get; set; } = null!;

    /// <summary>
    /// 1-based stop sequence in the route.
    /// </summary>
    public int StopOrder { get; set; }
}