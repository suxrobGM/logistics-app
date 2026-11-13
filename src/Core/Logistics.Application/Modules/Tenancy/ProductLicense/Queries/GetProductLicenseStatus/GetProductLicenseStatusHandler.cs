using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.ApplicationServices;
using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Tenancy.ProductLicense.Queries;

internal sealed class GetProductLicenseStatusHandler(IProductLicenseService license)
    : IRequestHandler<GetProductLicenseStatusQuery, Result<ProductLicenseStatusDto>>
{
    public async Task<Result<ProductLicenseStatusDto>> Handle(GetProductLicenseStatusQuery req, CancellationToken ct)
    {
        return Result<ProductLicenseStatusDto>.Ok(await license.GetStatusAsync(ct));
    }
}
