using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
/// Key-value store for system-wide configuration settings (master database).
/// Used for Stripe IDs, feature flags, and other infrastructure settings.
/// </summary>
public class SystemSetting : Entity, IMasterEntity
{
    public required string Key { get; set; }
    public required string Value { get; set; }
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
