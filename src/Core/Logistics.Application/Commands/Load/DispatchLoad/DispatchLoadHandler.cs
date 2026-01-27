using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DispatchLoadHandler(ITenantUnitOfWork tenantUow) : IAppRequestHandler<DispatchLoadCommand, Result>
{
    public async Task<Result> Handle(DispatchLoadCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.Id, ct);

        if (load is null)
        {
            return Result.Fail($"Load not found with ID '{req.Id}'");
        }

        load.Dispatch();

        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
