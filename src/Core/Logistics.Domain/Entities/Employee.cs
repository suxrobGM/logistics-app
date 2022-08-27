namespace Logistics.Domain.Entities;

public class Employee : Entity, ITenantEntity
{
    /// <summary>
    /// When employee joined to this tenant
    /// </summary>
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Dispatched loads by dispatchers
    /// </summary>
    public virtual IList<Load> DispatchedLoads { get; set; } = new List<Load>();
    
    /// <summary>
    /// Delivered loads by drivers
    /// </summary>
    public virtual IList<Load> DeliveredLoads { get; set; } = new List<Load>();
    
    /// <summary>
    /// User tenant roles
    /// </summary>
    public virtual ISet<TenantRole> Roles { get; set; } = new HashSet<TenantRole>(new TenantRoleComparer());
}
