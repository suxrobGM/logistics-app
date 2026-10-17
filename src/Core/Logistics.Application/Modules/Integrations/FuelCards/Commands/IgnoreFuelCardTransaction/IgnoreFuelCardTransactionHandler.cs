using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

internal sealed class IgnoreFuelCardTransactionHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<IgnoreFuelCardTransactionCommand, Result>
{
    public async Task<Result> Handle(IgnoreFuelCardTransactionCommand req, CancellationToken ct)
    {
        var transactions = await tenantUow.Repository<FuelCardTransaction>().GetListAsync(
            t => req.TransactionIds.Contains(t.Id) && t.Status != FuelCardTransactionStatus.Matched, ct);

        foreach (var transaction in transactions)
        {
            transaction.Status = FuelCardTransactionStatus.Ignored;
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
