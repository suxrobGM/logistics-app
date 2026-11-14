using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Expenses.Queries;

/// <summary><see cref="SearchableQuery.Search"/> matches the vendor name or notes.</summary>
public class GetExpensesQuery : SearchableQuery, IQuery<PagedResult<ExpenseDto>>
{
    public ExpenseType? Type { get; set; }
    public ExpenseStatus? Status { get; set; }
    public Guid? TruckId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
