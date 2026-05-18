using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Expenses.Queries;

public class GetExpenseByIdQuery : IQuery<Result<ExpenseDto>>
{
    public Guid Id { get; set; }
}
