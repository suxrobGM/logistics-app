using Logistics.Domain.ValueObjects;

namespace Logistics.Domain.Entities;

public class Employee : Entity, ITenantEntity
{
    public string? ExternalId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public DateTime JoinedDate { get; set; } = DateTime.Now;
    public EmployeeRole Role { get; set; } = EmployeeRole.Guest;

    /// <summary>
    /// Dispatched loads by dispatchers
    /// </summary>
    public virtual IList<Load> DispatchedLoads { get; set; } = new List<Load>();
    
    /// <summary>
    /// Delivered loads by drivers
    /// </summary>
    public virtual IList<Load> DeliveredLoads { get; set; } = new List<Load>();
}
