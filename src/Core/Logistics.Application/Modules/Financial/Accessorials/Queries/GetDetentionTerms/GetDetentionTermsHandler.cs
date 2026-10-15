using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Accessorials.Queries;

internal sealed class GetDetentionTermsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetDetentionTermsQuery, Result<DetentionTermsDto>>
{
    public Task<Result<DetentionTermsDto>> Handle(GetDetentionTermsQuery req, CancellationToken ct)
    {
        var settings = tenantUow.GetCurrentTenant().Settings;
        return Task.FromResult(Result<DetentionTermsDto>.Ok(new DetentionTermsDto
        {
            FreeMinutes = settings.DetentionFreeMinutes,
            RatePerHour = settings.DetentionRatePerHour
        }));
    }
}
