using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class RejectExpenseHandler : IAppRequestHandler<RejectExpenseCommand, Result>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public RejectExpenseHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<Result> Handle(RejectExpenseCommand req, CancellationToken ct)
    {
        var expense = await _tenantUow.Repository<Expense>().GetByIdAsync(req.Id);

        if (expense is null)
        {
            return Result.Fail($"Could not find expense with ID '{req.Id}'");
        }

        if (expense.Status != ExpenseStatus.Pending)
        {
            return Result.Fail("Only pending expenses can be rejected.");
        }

        expense.Reject(req.ApproverId, req.Reason);

        _tenantUow.Repository<Expense>().Update(expense);
        await _tenantUow.SaveChangesAsync();

        return Result.Ok();
    }
}
