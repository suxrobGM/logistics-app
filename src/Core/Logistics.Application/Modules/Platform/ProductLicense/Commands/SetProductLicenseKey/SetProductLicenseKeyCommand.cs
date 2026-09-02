using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.ProductLicense.Commands;

/// <summary>
/// Installs a commercial license key in the master database. SuperAdmin only. Rejected when the
/// key is managed by the License__Key setting instead.
/// </summary>
public sealed class SetProductLicenseKeyCommand : ICommand<Result<ProductLicenseStatusDto>>
{
    public string Key { get; set; } = string.Empty;
}
