namespace Logistics.Application.Abstractions.ProductLicense;

/// <summary>
/// Sends the daily heartbeat for this deployment. Runs as a global Hangfire job, no tenant fan-out.
/// </summary>
public interface IProductLicenseHeartbeatService
{
    /// <summary>
    /// Builds the report, posts it, and records the send when the receiver accepted it.
    /// Does nothing when the heartbeat is turned off.
    /// </summary>
    Task SendHeartbeatAsync(CancellationToken ct = default);
}
