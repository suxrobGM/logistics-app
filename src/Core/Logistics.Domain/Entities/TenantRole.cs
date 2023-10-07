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
    public string? NormalizedName { get; set; }
    
    public virtual List<Employee> Employees { get; } = new();
    public virtual List<EmployeeTenantRole> EmployeeRoles { get; } = new();
    public virtual HashSet<TenantRoleClaim> Claims { get; } = new(new TenantRoleClaimComparer());
}

internal class TenantRoleComparer : IEqualityComparer<TenantRole>
{
    public bool Equals(TenantRole? x, TenantRole? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Name == y.Name;
    }

    public int GetHashCode(TenantRole? obj)
    {
        return obj?.Name != null ? obj.Name.GetHashCode() : 0;
    }
}
