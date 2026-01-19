using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteExpenseHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<DeleteExpenseCommand, Result>
{
    public async Task<Result> Handle(DeleteExpenseCommand req, CancellationToken ct)
    {
        var expense = await tenantUow.Repository<Expense>().GetByIdAsync(req.Id, ct);

        if (expense is null)
        {
            return Result.Fail($"Could not find expense with ID '{req.Id}'");
        }

        // Only pending or rejected expenses can be deleted
        if (expense.Status == ExpenseStatus.Approved || expense.Status == ExpenseStatus.Paid)
        {
            return Result.Fail("Approved or paid expenses cannot be deleted.");
        }

        tenantUow.Repository<Expense>().Delete(expense);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
