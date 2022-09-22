using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class AppRole : IdentityRole, IAggregateRoot
{
    public AppRole(string name): base(name)
    {
        if (!name.StartsWith("app."))
            base.Name = $"app.{name}";
        
        DisplayName = base.Name;
    }
    
    [StringLength(RoleConsts.DisplayNameLength)]
    public string? DisplayName { get; set; }
}

internal class AppRoleComparer : IEqualityComparer<AppRole>
{
    public bool Equals(AppRole? x, AppRole? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Name == y.Name;
    }

    public int GetHashCode(AppRole obj)
    {
        return obj.Name != null ? obj.Name.GetHashCode() : 0;
    }
}