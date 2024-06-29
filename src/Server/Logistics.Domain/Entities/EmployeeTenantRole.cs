using System.ComponentModel.DataAnnotations.Schema;
using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class EmployeeTenantRole : ITenantEntity
{
    [NotMapped]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string EmployeeId { get; set; } = default!;
    public virtual Employee Employee { get; set; } = default!;
    
    public string RoleId { get; set; } = default!;
    public virtual TenantRole Role { get; set; } = default!;
}
