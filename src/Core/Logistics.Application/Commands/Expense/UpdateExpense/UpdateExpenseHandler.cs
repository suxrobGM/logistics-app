using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateExpenseHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdateExpenseCommand, Result>
{
    public async Task<Result> Handle(UpdateExpenseCommand req, CancellationToken ct)
    {
        var expense = await tenantUow.Repository<Expense>().GetByIdAsync(req.Id, ct);

        if (expense is null)
        {
            return Result.Fail($"Could not find expense with ID '{req.Id}'");
        }

        // Only pending expenses can be updated
        if (expense.Status != ExpenseStatus.Pending)
        {
            return Result.Fail("Only pending expenses can be updated.");
        }

        // Update common fields
        expense.Amount = new Money { Amount = req.Amount, Currency = req.Currency };
        expense.VendorName = req.VendorName;
        expense.ExpenseDate = req.ExpenseDate;
        expense.Notes = req.Notes;

        if (!string.IsNullOrEmpty(req.ReceiptBlobPath))
        {
            expense.ReceiptBlobPath = req.ReceiptBlobPath;
        }

        // Update type-specific fields
        switch (expense)
        {
            case CompanyExpense companyExpense when req.CompanyCategory.HasValue:
                companyExpense.Category = req.CompanyCategory.Value;
                break;

            case TruckExpense truckExpense:
                if (req.TruckCategory.HasValue)
                {
                    truckExpense.Category = req.TruckCategory.Value;
                }

                truckExpense.OdometerReading = req.OdometerReading;
                break;

            case BodyShopExpense bodyShopExpense:
                bodyShopExpense.VendorAddress = req.VendorAddress;
                bodyShopExpense.VendorPhone = req.VendorPhone;
                bodyShopExpense.RepairDescription = req.RepairDescription;
                bodyShopExpense.EstimatedCompletionDate = req.EstimatedCompletionDate;
                bodyShopExpense.ActualCompletionDate = req.ActualCompletionDate;
                break;
        }

        tenantUow.Repository<Expense>().Update(expense);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
