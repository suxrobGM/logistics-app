using Logistics.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Domain.Entities;

public class UserRole : IdentityRole<string>, IAggregateRoot
{
    public override string Id { get; set; } = Generator.NewGuid();
    public UserRoleType RoleType { get; set; }
}