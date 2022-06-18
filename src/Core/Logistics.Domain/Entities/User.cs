using Logistics.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class User : IdentityUser, IAggregateRoot
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public List<string> JoinedTenants { get; set; } = new();
    public DateTime JoinedDate { get; set; } = DateTime.Now;
    public UserRole Role { get; set; } = UserRole.Guest;

    public string GetFullName()
    {
        if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
        {
            return UserName;
        }
        return string.Join(" ", FirstName, LastName);
    }
}