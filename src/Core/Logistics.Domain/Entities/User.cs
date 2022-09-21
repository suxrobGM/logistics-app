using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class User : IdentityUser, IAggregateRoot
{
    [StringLength(UserConsts.NameLength)]
    public string? FirstName { get; set; }
    
    [StringLength(UserConsts.NameLength)]
    public string? LastName { get; set; }

    public string JoinedTenantIds { get; set; } = "";
    public DateTime RegistrationDate { get; } = DateTime.UtcNow;

    public void JoinTenant(string tenantId)
    {
        JoinedTenantIds = string.IsNullOrEmpty(JoinedTenantIds) ? 
            string.Join(',', tenantId) : string.Join(',', JoinedTenantIds, tenantId);
    }
    
    public void RemoveTenant(string tenantId)
    {
        JoinedTenantIds = JoinedTenantIds.Replace($",{tenantId}", "")
            .Replace(tenantId, "");
    }

    public string GetFullName()
    {
        if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
        {
            return UserName;
        }
        return string.Join(" ", FirstName, LastName);
    }
}