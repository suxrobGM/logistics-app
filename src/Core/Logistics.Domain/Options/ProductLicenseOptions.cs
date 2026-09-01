namespace Logistics.Domain.Options;

/// <summary>
/// Commercial license settings. Bound from the "License" section, so the environment
/// variables are License__Key, License__HeartbeatUrl and License__HeartbeatEnabled.
/// </summary>
public sealed class ProductLicenseOptions
{
    public const string SectionName = "License";

    /// <summary>
    /// Signed license key. When set it takes precedence over the key stored in the database.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Endpoint that receives the daily instance heartbeat.
    /// </summary>
    public string HeartbeatUrl { get; set; } = "https://api.logisticsx.app/license/heartbeat";

    /// <summary>
    /// Turns the daily heartbeat off. The author's own deployment sets this to false.
    /// </summary>
    public bool HeartbeatEnabled { get; set; } = true;
}
