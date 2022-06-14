using Logistics.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class User : IdentityUser, IAggregateRoot
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime JoinedDate { get; set; } = DateTime.Now;
    public UserRole Role { get; set; } = UserRole.Guest;

    public string GetFullName() => string.Join(" ", new[] { FirstName, LastName });
}