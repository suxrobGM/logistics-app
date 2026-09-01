using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
/// One row per deployed instance that reports to this API (master database). Updated in place
/// on every heartbeat, so the table stays as small as the number of known instances.
/// </summary>
public class LicenseHeartbeat : Entity, IMasterEntity
{
    public required Guid InstanceId { get; set; }
    public required string Hostname { get; set; }
    public required string Version { get; set; }
    public string? KeyId { get; set; }
    public string? Licensee { get; set; }
    public int TenantCount { get; set; }
    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    public void Touch(string hostname, string version, string? keyId, string? licensee, int tenantCount, DateTime nowUtc)
    {
        Hostname = hostname;
        Version = version;
        KeyId = keyId;
        Licensee = licensee;
        TenantCount = tenantCount;
        LastSeenAt = nowUtc;
    }
}
