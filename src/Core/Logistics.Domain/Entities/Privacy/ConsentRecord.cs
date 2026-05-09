using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// An individual cookie / processing consent decision, captured per ePrivacy directive.
/// Either UserId (authenticated) or AnonymousId (cookie-set GUID) is set.
/// </summary>
public class ConsentRecord : Entity, IMasterEntity
{
    /// <summary>
    /// Authenticated user; null for anonymous web visitors.
    /// </summary>
    public Guid? UserId { get; set; }
    public virtual User? User { get; set; }

    /// <summary>
    /// Browser-scoped GUID stored alongside the consent cookie; used when UserId is null.
    /// </summary>
    public Guid? AnonymousId { get; set; }

    public ConsentType ConsentType { get; set; }
    public bool Granted { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
