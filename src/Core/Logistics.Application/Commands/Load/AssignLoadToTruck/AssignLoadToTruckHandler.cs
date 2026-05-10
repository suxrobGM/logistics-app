using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class AssignLoadToTruckHandler(ITenantUnitOfWork tenantUow) : IAppRequestHandler<AssignLoadToTruckCommand, Result>
{
    public async Task<Result> Handle(AssignLoadToTruckCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);

        if (load is null)
        {
            return Result.Fail($"Load not found with ID '{req.LoadId}'");
        }

        load.AssignedTruckId = req.TruckId;
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
