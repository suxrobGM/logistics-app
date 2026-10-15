using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

internal sealed class ReleaseFactoringReserveHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<ReleaseFactoringReserveCommand, Result>
{
    public async Task<Result> Handle(ReleaseFactoringReserveCommand req, CancellationToken ct)
    {
        var item = await tenantUow.Repository<FactoringBatchItem>()
            .GetAsync(i => i.Id == req.ItemId && i.BatchId == req.BatchId, ct);
        if (item is null)
        {
            return Result.Fail("Invoice not found on this schedule.");
        }

        try
        {
            item.ReleaseReserve(req.Amount, DateTime.SpecifyKind(req.ReleasedAt, DateTimeKind.Utc));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
