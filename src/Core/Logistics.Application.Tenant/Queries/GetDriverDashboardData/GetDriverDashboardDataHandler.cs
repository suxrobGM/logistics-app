using Logistics.Models;

namespace Logistics.Application.Tenant.Queries.GetDriverDashboardData;

internal sealed class GetDriverDashboardDataHandler : RequestHandler<GetDriverDashboardDataQuery, ResponseResult<DriverDashboardDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetDriverDashboardDataHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    
    protected override Task<ResponseResult<DriverDashboardDto>> HandleValidated(GetDriverDashboardDataQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}