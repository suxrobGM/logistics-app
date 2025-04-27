using Logistics.Domain.Core;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class AppRoleClaim : IdentityRoleClaim<string>, IEntity<int>
{
    //public new string Id { get; set; } = Guid.NewGuid().ToString();
    public virtual AppRole Role { get; set; } = null!;
}

internal class AppRoleClaimComparer : IEqualityComparer<AppRoleClaim>
{
    public bool Equals(AppRoleClaim? x, AppRoleClaim? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.ClaimValue == y.ClaimValue && x.ClaimType == y.ClaimType;
    }

    public int GetHashCode(AppRoleClaim obj)
    {
        return obj.Id.GetHashCode();
    }
}
