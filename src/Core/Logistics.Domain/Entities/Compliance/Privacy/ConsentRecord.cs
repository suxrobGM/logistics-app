using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// An individual cookie / processing consent decision, captured per ePrivacy directive
/// for the authenticated user who made it.
/// </summary>
public class ConsentRecord : Entity, IMasterEntity
{
    public Guid UserId { get; set; }
    public virtual User? User { get; set; }

    public ConsentType ConsentType { get; set; }
    public bool Granted { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
