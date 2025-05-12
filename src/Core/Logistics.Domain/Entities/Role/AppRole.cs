using Logistics.Domain.Core;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class AppRole : IdentityRole<Guid>, IEntity<Guid>, IMasterEntity
{
    public AppRole(string name): base(name)
    {
        if (!name.StartsWith("app."))
            base.Name = $"app.{name}";
        
        DisplayName = base.Name;
    }

    public override Guid Id { get; set; } = Guid.NewGuid();
    public string? DisplayName { get; set; }
    
    public virtual HashSet<AppRoleClaim> Claims { get; } = new(new AppRoleClaimComparer());
}
