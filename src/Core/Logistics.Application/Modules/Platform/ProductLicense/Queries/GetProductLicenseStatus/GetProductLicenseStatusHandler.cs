using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.ProductLicense.Queries;

internal sealed class GetProductLicenseStatusHandler(IProductLicenseService license)
    : IAppRequestHandler<GetProductLicenseStatusQuery, Result<ProductLicenseStatusDto>>
{
    public async Task<Result<ProductLicenseStatusDto>> Handle(GetProductLicenseStatusQuery req, CancellationToken ct)
    {
        return Result<ProductLicenseStatusDto>.Ok(await license.GetStatusAsync(ct));
    }
}
