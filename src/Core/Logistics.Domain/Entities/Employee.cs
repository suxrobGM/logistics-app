namespace Logistics.Domain.Entities;

public class Employee : Entity, ITenantEntity
{
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    //public EmployeeRole Role2 { get; set; } = EmployeeRole.Guest;
    
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
