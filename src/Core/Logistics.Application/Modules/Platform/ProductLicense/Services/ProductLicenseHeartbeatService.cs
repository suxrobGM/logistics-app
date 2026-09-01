using System.Globalization;
using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Platform.ProductLicense.Services;

internal sealed class ProductLicenseHeartbeatService(
    IProductLicenseService license,
    IProductLicenseHeartbeatSender sender,
    ISystemSettingsService systemSettings,
    IOptions<ProductLicenseOptions> options,
    IMasterUnitOfWork masterUow) : IProductLicenseHeartbeatService, IApplicationService
{
    public async Task SendHeartbeatAsync(CancellationToken ct = default)
    {
        if (!options.Value.HeartbeatEnabled)
        {
            return;
        }

        var heartbeat = await BuildHeartbeatAsync(ct);
        if (await sender.SendAsync(heartbeat, ct))
        {
            await MarkSentAsync(ct);
        }
    }

    private async Task<ProductLicenseHeartbeatDto> BuildHeartbeatAsync(CancellationToken ct)
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

    private async Task MarkSentAsync(CancellationToken ct)
    {
        await systemSettings.SetAsync(ProductLicenseSettingsKeys.LastHeartbeatAt,
            DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            "When the license heartbeat receiver last accepted a report", ct);
        license.InvalidateCache();
    }
}
