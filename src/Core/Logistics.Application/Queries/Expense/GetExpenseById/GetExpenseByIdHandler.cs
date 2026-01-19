using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetExpenseByIdHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetExpenseByIdQuery, Result<ExpenseDto>>
{
    public async Task<Result<ExpenseDto>> Handle(GetExpenseByIdQuery req, CancellationToken ct)
    {
        var expense = await tenantUow.Repository<Expense>().GetByIdAsync(req.Id, ct);

        if (expense is null)
        {
            return Result<ExpenseDto>.Fail($"Could not find expense with ID '{req.Id}'");
        }

        return Result<ExpenseDto>.Ok(expense.ToDto());
    }
}
