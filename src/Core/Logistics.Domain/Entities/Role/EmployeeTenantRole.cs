using System.ComponentModel.DataAnnotations.Schema;

using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class EmployeeTenantRole : IEntity<Guid>, ITenantEntity
{
    [NotMapped]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;

    public Guid RoleId { get; set; }
    public virtual TenantRole Role { get; set; } = null!;
}
