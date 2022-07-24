using Logistics.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class User : IdentityUser, IAggregateRoot
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string JoinedTenantIds { get; set; } = "";
    public DateTime JoinedDate { get; set; } = DateTime.Now;
    public UserRole Role { get; set; } = UserRole.Guest;

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