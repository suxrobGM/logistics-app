using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class AppRole : IdentityRole<string>, IEntity<string>, IAuditableEntity
{
    public AppRole(string name): base(name)
    {
        if (!name.StartsWith("app."))
            base.Name = $"app.{name}";
        
        DisplayName = base.Name;
    }

    public override string Id { get; set; } = Guid.NewGuid().ToString();

    [StringLength(RoleConsts.DisplayNameLength)]
    public string? DisplayName { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}