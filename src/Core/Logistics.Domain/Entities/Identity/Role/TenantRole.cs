using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class TenantRole : Entity, ITenantEntity
{
    public TenantRole(string name)
    {
        if (!name.StartsWith("tenant."))
            name = $"tenant.{name}";

        Name = name;
        DisplayName = name;
        NormalizedName = name.ToUpper();
    }

    public string Name { get; set; }
    public string? DisplayName { get; set; }
    public string NormalizedName { get; set; }

    public virtual List<Employee> Employees { get; } = [];
    public virtual HashSet<TenantRoleClaim> Claims { get; } = [with(new TenantRoleClaimComparer())];
}
