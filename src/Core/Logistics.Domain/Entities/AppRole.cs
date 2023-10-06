using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class AppRole : IdentityRole<string>, IEntity<string>
{
    public AppRole(string name): base(name)
    {
        if (!name.StartsWith("app."))
            base.Name = $"app.{name}";
        
        DisplayName = base.Name;
    }

    public override string Id { get; set; } = Guid.NewGuid().ToString();
    public string? DisplayName { get; set; }
}
