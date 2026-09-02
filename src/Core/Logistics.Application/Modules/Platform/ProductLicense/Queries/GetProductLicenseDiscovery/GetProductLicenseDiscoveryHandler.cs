using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.ProductLicense.Queries;

internal sealed class GetProductLicenseDiscoveryHandler(IProductLicenseService license)
    : IAppRequestHandler<GetProductLicenseDiscoveryQuery, Result<ProductLicenseDiscoveryDto>>
{
    public async Task<Result<ProductLicenseDiscoveryDto>> Handle(GetProductLicenseDiscoveryQuery req, CancellationToken ct)
    {
        var status = await license.GetStatusAsync(ct);

        return Result<ProductLicenseDiscoveryDto>.Ok(new ProductLicenseDiscoveryDto
        {
            Product = ProductInfo.Name,
            Version = status.Version,
            Licensed = status.IsLicensed,
            Licensee = status.IsLicensed ? status.Licensee : null
        });
    }
}
