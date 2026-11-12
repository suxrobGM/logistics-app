using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

internal sealed class LinkContainerToLoadHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<LinkContainerToLoadCommand, Result>
{
    public async Task<Result> Handle(LinkContainerToLoadCommand req, CancellationToken ct)
    {
        var container = await tenantUow.Repository<Container>().GetByIdAsync(req.ContainerId, ct);
        if (container is null)
        {
            return Result.Fail($"Could not find container with ID '{req.ContainerId}'", ErrorCodes.NotFound);
        }

        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null)
        {
            return Result.Fail($"Could not find load with ID '{req.LoadId}'", ErrorCodes.NotFound);
        }

        load.ContainerId = container.Id;
        load.Container = container;

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
