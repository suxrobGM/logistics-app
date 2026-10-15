using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Accessorials.Commands;

internal sealed class UpdateDetentionTermsHandler(IMasterUnitOfWork masterUow, ITenantUnitOfWork tenantUow)
    : IRequestHandler<UpdateDetentionTermsCommand, Result>
{
    public async Task<Result> Handle(UpdateDetentionTermsCommand req, CancellationToken ct)
    {
        var tenantId = tenantUow.GetCurrentTenant().Id;
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenantId, ct);
        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{tenantId}'");
        }

        tenant.Settings = tenant.Settings with
        {
            DetentionFreeMinutes = req.FreeMinutes,
            DetentionRatePerHour = req.RatePerHour
        };
        masterUow.Repository<Tenant>().Update(tenant);
        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
