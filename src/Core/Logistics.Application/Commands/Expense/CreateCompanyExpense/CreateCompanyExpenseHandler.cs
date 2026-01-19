using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateCompanyExpenseHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreateCompanyExpenseCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCompanyExpenseCommand req, CancellationToken ct)
    {
        var expense = new CompanyExpense
        {
            Amount = new Money { Amount = req.Amount, Currency = req.Currency },
            VendorName = req.VendorName,
            ExpenseDate = req.ExpenseDate,
            ReceiptBlobPath = req.ReceiptBlobPath,
            Notes = req.Notes,
            Category = req.Category
        };

        await tenantUow.Repository<Expense>().AddAsync(expense, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<Guid>.Ok(expense.Id);
    }
}
