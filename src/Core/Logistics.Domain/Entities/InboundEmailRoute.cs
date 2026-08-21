using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Maps a reply-address token to the tenant that owns the thread (master database). Inbound mail is
/// resolved here before any tenant database is opened.
/// </summary>
public class InboundEmailRoute : Entity, IMasterEntity
{
    public required string ThreadToken { get; set; }
    public required Guid TenantId { get; set; }
    public InboundEmailPurpose Purpose { get; set; } = InboundEmailPurpose.RateNegotiation;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
