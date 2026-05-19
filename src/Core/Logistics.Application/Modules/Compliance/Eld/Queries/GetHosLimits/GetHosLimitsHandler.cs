using Logistics.Application.Modules.Compliance.Eld.Services;
using Logistics.Application.Abstractions;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Queries;

internal sealed class GetHosLimitsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetHosLimitsQuery, Result<HosLimitsDto>>
{
    public Task<Result<HosLimitsDto>> Handle(GetHosLimitsQuery req, CancellationToken ct)
    {
        var region = tenantUow.GetCurrentTenant().Settings.Region;
        var dto = RuleSetSelector.LimitsFor(region).ToDto();
        return Task.FromResult(Result<HosLimitsDto>.Ok(dto));
    }
}
