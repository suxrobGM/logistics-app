using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics.Domain.Entities;

public class EmployeeTenantRole : ITenantEntity
{
    [NotMapped]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string? EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }
    
    public string? RoleId { get; set; }
    public virtual TenantRole? Role { get; set; }
}
