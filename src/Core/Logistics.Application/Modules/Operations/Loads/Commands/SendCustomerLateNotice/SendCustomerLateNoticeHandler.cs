using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class SendCustomerLateNoticeHandler(
    ITenantUnitOfWork tenantUow,
    ICustomerUpdateService customerUpdates) : IRequestHandler<SendCustomerLateNoticeCommand, Result>
{
    public async Task<Result> Handle(SendCustomerLateNoticeCommand req, CancellationToken ct)
    {
        await tenantUow.SetCurrentTenantByIdAsync(req.TenantId);

        var stop = await tenantUow.Repository<TripStop>().GetByIdAsync(req.StopId, ct);
        if (stop is null)
        {
            return Result.Ok();
        }

        await customerUpdates.SendLateNoticeAsync(stop, req.EstimatedArrivalAt, ct);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
