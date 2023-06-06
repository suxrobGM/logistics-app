using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class User : IdentityUser, IAggregateRoot
{
    [StringLength(UserConsts.NameLength)]
    public string? FirstName { get; set; }
    
    [StringLength(UserConsts.NameLength)]
    public string? LastName { get; set; }

    public ISet<string> JoinedTenantIds { get; set; } = new HashSet<string>();
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    public string GetFullName()
    {
        if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
        {
            return UserName!;
        }
        return string.Join(" ", FirstName, LastName);
    }
}