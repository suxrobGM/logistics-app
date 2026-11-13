using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
/// API key for external integrations (MCP, API access).
/// Stored in the tenant database - each tenant manages their own keys.
/// </summary>
public class ApiKey : Entity, ITenantEntity
{
    public required string Name { get; set; }
    public required string KeyHash { get; set; }
    public required string KeyPrefix { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
}
