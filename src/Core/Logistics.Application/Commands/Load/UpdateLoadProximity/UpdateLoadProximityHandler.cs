using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateLoadProximityHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdateLoadProximityCommand, Result>
{
    public async Task<Result> Handle(UpdateLoadProximityCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId);

        if (load is null)
        {
            return Result.Fail($"Could not find load with ID '{req.LoadId}'");
        }

        // UpdateProximity raises LoadProximityChangedEvent for notifications
        load.UpdateProximity(req.CanConfirmPickUp, req.CanConfirmDelivery);

        tenantUow.Repository<Load>().Update(load);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
