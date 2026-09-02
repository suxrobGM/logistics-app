namespace Logistics.Application.Abstractions.ProductLicense;

/// <summary>
/// <see cref="SystemSettings"/> keys for the commercial license state.
/// </summary>
public static class ProductLicenseSettingsKeys
{
    /// <summary>The signed license key pasted by a SuperAdmin. Ignored when License__Key is set.</summary>
    public const string Key = "ProductLicense.Key";

    /// <summary>Random id generated on first use that identifies this deployment in heartbeats.</summary>
    public const string InstanceId = "ProductLicense.InstanceId";

    /// <summary>UTC timestamp (round-trip format) of the last heartbeat the receiver accepted.</summary>
    public const string LastHeartbeatAt = "ProductLicense.LastHeartbeatAt";
}
