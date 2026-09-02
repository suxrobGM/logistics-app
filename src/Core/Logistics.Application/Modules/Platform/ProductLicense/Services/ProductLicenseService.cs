using System.Globalization;
using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Domain.Options;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Platform.ProductLicense.Services;

internal sealed class ProductLicenseService(
    ISystemSettingsService systemSettings,
    IOptions<ProductLicenseOptions> options,
    ProductLicenseKeyValidator validator,
    IMemoryCache cache) : IProductLicenseService, IApplicationService
{
    private const string CacheKey = "ProductLicense.Status";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public async Task<ProductLicenseStatusDto> GetStatusAsync(CancellationToken ct = default)
    {
        if (cache.TryGetValue(CacheKey, out ProductLicenseStatusDto? cached) && cached is not null)
        {
            return cached;
        }

        var (key, source) = await ResolveKeyAsync(ct);
        var status = new ProductLicenseStatusDto
        {
            Source = source,
            Version = ProductInfo.Version,
            InstanceId = ParseGuid(await systemSettings.GetAsync(ProductLicenseSettingsKeys.InstanceId, ct)),
            LastHeartbeatAt = ParseUtc(await systemSettings.GetAsync(ProductLicenseSettingsKeys.LastHeartbeatAt, ct))
        };

        if (key is null)
        {
            status.Error = "no license key";
        }
        else
        {
            var validation = await validator.ValidateAsync(key);
            status.IsLicensed = validation.IsValid;
            status.Error = validation.Error;
            status.Licensee = validation.Licensee;
            status.Tier = validation.Tier;
            status.ExpiresAt = validation.ExpiresAt;
            status.MaxTenants = validation.MaxTenants;
            status.KeyId = validation.KeyId;
        }

        cache.Set(CacheKey, status, CacheTtl);
        return status;
    }

    public async Task<Guid> GetOrCreateInstanceIdAsync(CancellationToken ct = default)
    {
        var existing = ParseGuid(await systemSettings.GetAsync(ProductLicenseSettingsKeys.InstanceId, ct));
        if (existing is { } id)
        {
            return id;
        }

        id = Guid.NewGuid();
        await systemSettings.SetAsync(ProductLicenseSettingsKeys.InstanceId, id.ToString(),
            "Random id identifying this deployment in license heartbeats", ct);
        InvalidateCache();
        return id;
    }

    public void InvalidateCache() => cache.Remove(CacheKey);

    private async Task<(string? Key, ProductLicenseKeySource Source)> ResolveKeyAsync(CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(options.Value.Key))
        {
            return (options.Value.Key, ProductLicenseKeySource.Configuration);
        }

        var stored = await systemSettings.GetAsync(ProductLicenseSettingsKeys.Key, ct);
        return string.IsNullOrWhiteSpace(stored)
            ? (null, ProductLicenseKeySource.None)
            : (stored, ProductLicenseKeySource.SystemSettings);
    }

    private static Guid? ParseGuid(string? value) =>
        Guid.TryParse(value, out var id) ? id : null;

    private static DateTime? ParseUtc(string? value) =>
        DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var at)
            ? at.ToUniversalTime()
            : null;
}
