using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.ProductLicense;

/// <summary>
/// Builds the daily heartbeat payload and records when the receiver accepted it.
/// </summary>
public interface IProductLicenseHeartbeatService
{
    Task<ProductLicenseHeartbeatDto> BuildHeartbeatAsync(CancellationToken ct = default);

    /// <summary>When the last heartbeat was accepted, or null if none was.</summary>
    Task<DateTime?> GetLastSentAtAsync(CancellationToken ct = default);

    Task MarkSentAsync(CancellationToken ct = default);
}
