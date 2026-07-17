using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.FuelCards.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

internal sealed class AssignFuelCardTransactionTruckHandler(
    ITenantUnitOfWork tenantUow,
    ILogger<AssignFuelCardTransactionTruckHandler> logger)
    : IAppRequestHandler<AssignFuelCardTransactionTruckCommand, Result>
{
    public async Task<Result> Handle(AssignFuelCardTransactionTruckCommand req, CancellationToken ct)
    {
        var transaction = await tenantUow.Repository<FuelCardTransaction>().GetByIdAsync(req.TransactionId, ct);
        if (transaction is null)
        {
            return Result.Fail("Fuel card transaction not found");
        }

        if (transaction.Status == FuelCardTransactionStatus.Matched)
        {
            return Result.Fail("Transaction is already matched to a truck");
        }

        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);
        if (truck is null)
        {
            return Result.Fail("Truck not found");
        }

        var expense = FuelCardExpenseFactory.CreateExpense(transaction, truck.Id);
        await tenantUow.Repository<Expense>().AddAsync(expense, ct);

        transaction.TruckId = truck.Id;
        transaction.ExpenseId = expense.Id;
        transaction.Status = FuelCardTransactionStatus.Matched;

        if (req.RememberMapping && (transaction.ExternalCardId ?? transaction.CardNumberMasked) is { } cardId)
        {
            var cardRepo = tenantUow.Repository<FuelCard>();
            var card = await cardRepo.GetAsync(
                c => c.ProviderType == transaction.ProviderType && c.ExternalCardId == cardId, ct);

            if (card is null)
            {
                await cardRepo.AddAsync(new FuelCard
                {
                    ProviderType = transaction.ProviderType,
                    ExternalCardId = cardId,
                    UnitNumber = transaction.UnitNumber,
                    TruckId = truck.Id
                }, ct);
            }
            else
            {
                card.TruckId = truck.Id;
            }
        }

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Assigned fuel card transaction {TransactionId} to truck {TruckNumber}",
            transaction.Id, truck.Number);
        return Result.Ok();
    }
}
