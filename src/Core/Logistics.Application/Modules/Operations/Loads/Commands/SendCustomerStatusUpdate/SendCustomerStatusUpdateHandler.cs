using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class SendCustomerStatusUpdateHandler(
    ITenantUnitOfWork tenantUow,
    ICustomerUpdateService customerUpdates) : IRequestHandler<SendCustomerStatusUpdateCommand, Result>
{
    public async Task<Result> Handle(SendCustomerStatusUpdateCommand req, CancellationToken ct)
    {
        await tenantUow.SetCurrentTenantByIdAsync(req.TenantId);

        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null)
        {
            return Result.Ok();
        }

        await customerUpdates.SendStatusUpdateAsync(load, req.NewStatus, ct);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
