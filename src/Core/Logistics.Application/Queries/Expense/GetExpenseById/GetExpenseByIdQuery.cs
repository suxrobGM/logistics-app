using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetExpenseByIdQuery : IQuery<Result<ExpenseDto>>
{
    public Guid Id { get; set; }
}
