using System.ComponentModel.DataAnnotations.Schema;
using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class EmployeeTenantRole : ITenantEntity
{
    [NotMapped]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string EmployeeId { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    
    public string RoleId { get; set; } = null!;
    public virtual TenantRole Role { get; set; } = null!;
}
