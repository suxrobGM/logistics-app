using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteContainerHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<DeleteContainerCommand, Result>
{
    public async Task<Result> Handle(DeleteContainerCommand req, CancellationToken ct)
    {
        var container = await tenantUow.Repository<Container>().GetByIdAsync(req.Id, ct);

        if (container is null)
        {
            return Result.Fail($"Could not find container with ID '{req.Id}'");
        }

        var linkedLoad = await tenantUow.Repository<Load>().GetAsync(i => i.ContainerId == container.Id, ct);
        if (linkedLoad is not null)
        {
            return Result.Fail("Container is linked to one or more loads and cannot be deleted");
        }

        tenantUow.Repository<Container>().Delete(container);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
