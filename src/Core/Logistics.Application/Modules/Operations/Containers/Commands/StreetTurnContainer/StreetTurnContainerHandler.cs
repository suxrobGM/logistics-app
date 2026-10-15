using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

internal sealed class StreetTurnContainerHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<StreetTurnContainerCommand, Result>
{
    public async Task<Result> Handle(StreetTurnContainerCommand req, CancellationToken ct)
    {
        var container = await tenantUow.Repository<Container>().GetByIdAsync(req.Id, ct);
        if (container is null)
        {
            return Result.Fail($"Could not find container with ID '{req.Id}'");
        }

        try
        {
            container.MarkStreetTurned(req.ExportBookingReference, DateTime.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }

        tenantUow.Repository<Container>().Update(container);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
