using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

internal sealed class UpdateContainerHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdateContainerCommand, Result>
{
    public async Task<Result> Handle(UpdateContainerCommand req, CancellationToken ct)
    {
        var container = await tenantUow.Repository<Container>().GetByIdAsync(req.Id, ct);

        if (container is null)
        {
            return Result.Fail($"Could not find container with ID '{req.Id}'");
        }

        // Canonical vs canonical: a case-only edit is not a change, and the lookup needs the
        // stored form to find anything.
        var number = Container.NormalizeNumber(req.Number);
        if (!string.IsNullOrEmpty(number) && number != container.Number)
        {
            var conflict = await tenantUow.Repository<Container>().GetAsync(i => i.Number == number, ct);
            if (conflict is not null && conflict.Id != container.Id)
            {
                return Result.Fail($"A container with number '{number}' already exists");
            }
        }

        container.Number = PropertyUpdater.UpdateIfChanged(req.Number, container.Number);
        container.IsoType = PropertyUpdater.UpdateIfChanged(req.IsoType, container.IsoType);
        container.SealNumber = PropertyUpdater.UpdateIfChanged(req.SealNumber, container.SealNumber);
        container.BookingReference = PropertyUpdater.UpdateIfChanged(req.BookingReference, container.BookingReference);
        container.BillOfLadingNumber = PropertyUpdater.UpdateIfChanged(req.BillOfLadingNumber, container.BillOfLadingNumber);
        container.GrossWeight = PropertyUpdater.UpdateIfChanged(req.GrossWeight, container.GrossWeight);
        container.Notes = PropertyUpdater.UpdateIfChanged(req.Notes, container.Notes);

        tenantUow.Repository<Container>().Update(container);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
