using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.ProductLicense;

/// <summary>
/// Posts one heartbeat to the author's receiver. Never throws: a receiver outage must not
/// surface as a failed job.
/// </summary>
public interface IProductLicenseHeartbeatSender
{
    /// <summary>True when the receiver accepted the report.</summary>
    Task<bool> SendAsync(ProductLicenseHeartbeatDto heartbeat, CancellationToken ct = default);
}
