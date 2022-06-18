using Logistics.Domain.ValueObjects;

namespace Logistics.Domain.Entities;

public class Employee : Entity, ITenantEntity
{
    public string? ExternalId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public DateTime JoinedDate { get; set; } = DateTime.Now;
    public EmployeeRole Role { get; set; } = EmployeeRole.Guest;

    public string GetFullName() => string.Join(" ", new[] {FirstName, LastName});
    public virtual IList<Cargo> DispatcherCargoes { get; set; } = new List<Cargo>();
}
