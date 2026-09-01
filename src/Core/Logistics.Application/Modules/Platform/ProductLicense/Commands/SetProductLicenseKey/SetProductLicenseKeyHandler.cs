using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Application.Modules.Platform.ProductLicense.Services;
using Logistics.Domain.Options;
using Logistics.Shared.Models;
using Microsoft.Extensions.Options;

namespace Logistics.Application.Modules.Platform.ProductLicense.Commands;

internal sealed class SetProductLicenseKeyHandler(
    ISystemSettingsService systemSettings,
    IProductLicenseService license,
    ProductLicenseKeyValidator validator,
    IOptions<ProductLicenseOptions> options)
    : IAppRequestHandler<SetProductLicenseKeyCommand, Result<ProductLicenseStatusDto>>
{
    public async Task<Result<ProductLicenseStatusDto>> Handle(SetProductLicenseKeyCommand req, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(options.Value.Key))
        {
            return Result<ProductLicenseStatusDto>.Fail(
                "The license key is managed by the License__Key setting. Change it there and restart the API.");
        }

        var key = req.Key.Trim();
        var validation = await validator.ValidateAsync(key);
        if (!validation.IsValid)
        {
            return Result<ProductLicenseStatusDto>.Fail($"License key rejected: {validation.Error}.");
        }

        await systemSettings.SetAsync(ProductLicenseSettingsKeys.Key, key, "Commercial license key", ct);
        license.InvalidateCache();

        return Result<ProductLicenseStatusDto>.Ok(await license.GetStatusAsync(ct));
    }
}
