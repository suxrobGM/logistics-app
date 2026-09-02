using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
/// Full license state for the admin portal. Tenant users never see this; they read
/// <see cref="ProductLicenseDiscoveryDto"/> instead.
/// </summary>
public record ProductLicenseStatusDto
{
    /// <summary>True only when a key is present, correctly signed, and not expired.</summary>
    public bool IsLicensed { get; set; }

    public string? Licensee { get; set; }
    public ProductLicenseTier? Tier { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxTenants { get; set; }

    /// <summary>The "kid" header of the key, used to identify the signing key generation.</summary>
    public string? KeyId { get; set; }

    public ProductLicenseKeySource Source { get; set; }

    /// <summary>Why the key is not valid, e.g. "expired" or "invalid signature". Null when licensed.</summary>
    public string? Error { get; set; }

    public string Version { get; set; } = string.Empty;
    public Guid? InstanceId { get; set; }
    public DateTime? LastHeartbeatAt { get; set; }
}
