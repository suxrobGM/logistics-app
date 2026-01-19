using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateBodyShopExpenseHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreateBodyShopExpenseCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBodyShopExpenseCommand req, CancellationToken ct)
    {
        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);

        if (truck is null)
        {
            return Result<Guid>.Fail($"Could not find truck with ID '{req.TruckId}'");
        }

        var expense = new BodyShopExpense
        {
            Amount = new Money { Amount = req.Amount, Currency = req.Currency },
            VendorName = req.VendorName,
            ExpenseDate = req.ExpenseDate,
            ReceiptBlobPath = req.ReceiptBlobPath,
            Notes = req.Notes,
            TruckId = req.TruckId,
            VendorAddress = req.VendorAddress,
            VendorPhone = req.VendorPhone,
            RepairDescription = req.RepairDescription,
            EstimatedCompletionDate = req.EstimatedCompletionDate,
            ActualCompletionDate = req.ActualCompletionDate
        };

        await tenantUow.Repository<Expense>().AddAsync(expense, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<Guid>.Ok(expense.Id);
    }
}
