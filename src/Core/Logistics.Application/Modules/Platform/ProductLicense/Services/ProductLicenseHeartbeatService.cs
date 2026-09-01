using System.Globalization;
using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.ProductLicense.Services;

internal sealed class ProductLicenseHeartbeatService(
    IProductLicenseService license,
    ISystemSettingsService systemSettings,
    IMasterUnitOfWork masterUow) : IProductLicenseHeartbeatService, IApplicationService
{
    public async Task<ProductLicenseHeartbeatDto> BuildHeartbeatAsync(CancellationToken ct = default)
    {
        var instanceId = await license.GetOrCreateInstanceIdAsync(ct);
        var status = await license.GetStatusAsync(ct);
        var tenantCount = await masterUow.Repository<Tenant>().CountAsync(ct: ct);

        return new ProductLicenseHeartbeatDto
        {
            InstanceId = instanceId,
            Hostname = Environment.MachineName,
            Version = ProductInfo.Version,
            KeyId = status.IsLicensed ? status.KeyId : null,
            Licensee = status.IsLicensed ? status.Licensee : null,
            TenantCount = tenantCount
        };
    }

    public async Task<DateTime?> GetLastSentAtAsync(CancellationToken ct = default)
    {
        var value = await systemSettings.GetAsync(ProductLicenseSettingsKeys.LastHeartbeatAt, ct);
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var at)
            ? at.ToUniversalTime()
            : null;
    }

    public async Task MarkSentAsync(DateTime sentAtUtc, CancellationToken ct = default)
    {
        await systemSettings.SetAsync(ProductLicenseSettingsKeys.LastHeartbeatAt,
            sentAtUtc.ToString("o", CultureInfo.InvariantCulture),
            "When the license heartbeat receiver last accepted a report", ct);
        license.InvalidateCache();
    }
}
