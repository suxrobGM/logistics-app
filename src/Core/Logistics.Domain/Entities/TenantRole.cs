namespace Logistics.Domain.Entities;

public class TenantRole : Entity, ITenantEntity
{
    public TenantRole()
    {
    }
    
    public TenantRole(string name)
    {
        Name = name;
    }
    
    public string? Name { get; set; }
    public virtual IList<Employee> Employees { get; set; } = new List<Employee>();
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

    public int GetHashCode(TenantRole obj)
    {
        return obj.Name != null ? obj.Name.GetHashCode() : 0;
    }
}