using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class AppRole : IdentityRole<string>, IAggregateRoot
{
    public AppRole(string name): base(name)
    {
        if (!name.StartsWith("app."))
            base.Name = $"app.{name}";
        
        DisplayName = base.Name;
    }

    public override string Id { get; set; } = Generator.NewGuid();

    [StringLength(RoleConsts.DisplayNameLength)]
    public string? DisplayName { get; set; }
}