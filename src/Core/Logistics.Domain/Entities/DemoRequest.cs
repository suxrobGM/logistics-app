using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class DemoRequest : Entity, IMasterEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Company { get; set; }
    public string? Phone { get; set; }
    public string? FleetSize { get; set; }
    public string? Message { get; set; }
    public string? Notes { get; set; }
    public DemoRequestStatus Status { get; set; } = DemoRequestStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
