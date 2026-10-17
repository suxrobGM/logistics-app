using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.FuelCards.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

internal sealed class AssignFuelCardTransactionTruckHandler(
    ITenantUnitOfWork tenantUow,
    ILogger<AssignFuelCardTransactionTruckHandler> logger)
    : IRequestHandler<AssignFuelCardTransactionTruckCommand, Result>
{
    public async Task<Result> Handle(AssignFuelCardTransactionTruckCommand req, CancellationToken ct)
    {
        var transactions = (await tenantUow.Repository<FuelCardTransaction>()
                .GetListAsync(t => req.TransactionIds.Contains(t.Id), ct))
            .Where(t => t.Status != FuelCardTransactionStatus.Matched)
            .ToList();

        if (transactions.Count == 0)
        {
            return Result.Fail("No pending fuel card transactions to assign");
        }

        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);
        if (truck is null)
        {
            return Result.Fail("Truck not found");
        }

        // Several selected transactions can share a card; a card added earlier in this batch is not queryable yet.
        var mappedCards = new HashSet<(FuelCardProviderType, string)>();

        foreach (var transaction in transactions)
        {
            var expense = FuelCardExpenseFactory.CreateExpense(transaction, truck.Id);
            await tenantUow.Repository<Expense>().AddAsync(expense, ct);

            transaction.TruckId = truck.Id;
            transaction.ExpenseId = expense.Id;
            transaction.Status = FuelCardTransactionStatus.Matched;

            if (req.RememberMapping && (transaction.ExternalCardId ?? transaction.CardNumberMasked) is { } cardId)
            {
                await RememberCardAsync(transaction, cardId, truck, mappedCards, ct);
            }
        }

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Assigned {Count} fuel card transactions to truck {TruckNumber}",
            transactions.Count, truck.Number);
        return Result.Ok();
    }

    private async Task RememberCardAsync(
        FuelCardTransaction transaction,
        string cardId,
        Truck truck,
        HashSet<(FuelCardProviderType, string)> mappedCards,
        CancellationToken ct)
    {
        if (!mappedCards.Add((transaction.ProviderType, cardId)))
        {
            return;
        }

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
}
