using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class LinkContainerToLoadHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<LinkContainerToLoadCommand, Result>
{
    public async Task<Result> Handle(LinkContainerToLoadCommand req, CancellationToken ct)
    {
        var container = await tenantUow.Repository<Container>().GetByIdAsync(req.ContainerId, ct);
        if (container is null)
        {
            return Result.Fail($"Could not find container with ID '{req.ContainerId}'");
        }

        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null)
        {
            return Result.Fail($"Could not find load with ID '{req.LoadId}'");
        }

        load.ContainerId = container.Id;
        load.Container = container;

        tenantUow.Repository<Load>().Update(load);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
