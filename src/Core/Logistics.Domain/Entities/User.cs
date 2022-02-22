using Logistics.Domain.ValueObjects;

namespace Logistics.Domain.Entities;

public class User : Entity
{
    public string? ExternalId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime JoinedDate { get; set; } = DateTime.Now;
    public UserRoleType RoleType { get; set; }
}