using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

internal sealed class IgnoreFuelCardTransactionHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<IgnoreFuelCardTransactionCommand, Result>
{
    public async Task<Result> Handle(IgnoreFuelCardTransactionCommand req, CancellationToken ct)
    {
        var transaction = await tenantUow.Repository<FuelCardTransaction>().GetByIdAsync(req.TransactionId, ct);
        if (transaction is null)
        {
            return Result.Fail("Fuel card transaction not found");
        }

        if (transaction.Status == FuelCardTransactionStatus.Matched)
        {
            return Result.Fail("Transaction is already matched to a truck and cannot be ignored");
        }

        transaction.Status = FuelCardTransactionStatus.Ignored;
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
