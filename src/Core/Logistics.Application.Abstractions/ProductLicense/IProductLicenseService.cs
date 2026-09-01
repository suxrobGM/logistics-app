using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.ProductLicense;

/// <summary>
/// Resolves and validates the commercial license key for this deployment.
/// </summary>
public interface IProductLicenseService
{
    /// <summary>
    /// Current license status. Cached for a few minutes because the discovery endpoint and the
    /// portal banner read it on every app start.
    /// </summary>
    Task<ProductLicenseStatusDto> GetStatusAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the stable instance id, creating and storing one on first call.
    /// </summary>
    Task<Guid> GetOrCreateInstanceIdAsync(CancellationToken ct = default);

    /// <summary>
    /// Drops the cached status after the key changes.
    /// </summary>
    void InvalidateCache();
}
