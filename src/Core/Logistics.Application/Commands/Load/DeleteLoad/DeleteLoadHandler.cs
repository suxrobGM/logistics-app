using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteLoadHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<DeleteLoadCommand, Result>
{
    public async Task<Result> Handle(DeleteLoadCommand req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.Id);

        if (load is null)
        {
            return Result.Fail("Could not find the specified load");
        }

        // Raise domain event for truck notification before deletion
        load.MarkRemovedFromTruck();

        tenantUow.Repository<Load>().Delete(load);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
