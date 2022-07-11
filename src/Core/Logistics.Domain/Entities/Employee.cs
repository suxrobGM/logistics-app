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

    public virtual IList<Load> DispatcherCargoes { get; set; } = new List<Load>();
    
    public string GetFullName()
    {
        if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
        {
            return UserName!;
        }
        return string.Join(" ", FirstName, LastName);
    }
}
