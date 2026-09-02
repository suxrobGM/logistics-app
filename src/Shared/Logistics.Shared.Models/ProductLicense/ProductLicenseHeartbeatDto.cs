namespace Logistics.Shared.Models;

/// <summary>
/// Daily report each deployed instance sends to the author's API. Documented in
/// COMMERCIAL-LICENSE.md; nothing beyond these fields is sent.
/// </summary>
public record ProductLicenseHeartbeatDto
{
    public Guid InstanceId { get; set; }
    public string Hostname { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? KeyId { get; set; }
    public string? Licensee { get; set; }
    public int TenantCount { get; set; }
}
