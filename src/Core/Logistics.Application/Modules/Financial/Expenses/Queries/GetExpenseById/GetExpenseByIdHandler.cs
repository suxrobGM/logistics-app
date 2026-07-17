using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Expenses.Queries;

internal sealed class GetExpenseByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetExpenseByIdQuery, Expense, ExpenseDto>(tenantUow)
{
    protected override ExpenseDto MapToDto(Expense entity) => entity.ToDto();
}
