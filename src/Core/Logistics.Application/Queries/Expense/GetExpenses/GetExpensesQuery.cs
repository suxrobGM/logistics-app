using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetExpensesQuery : PagedQuery, IAppRequest<PagedResult<ExpenseDto>>
{
    /// <summary>
    ///     Filter expenses by type (Company, Truck, BodyShop)
    /// </summary>
    public ExpenseType? Type { get; set; }

    /// <summary>
    ///     Filter expenses by status
    /// </summary>
    public ExpenseStatus? Status { get; set; }

    /// <summary>
    ///     Filter expenses by truck ID (for Truck and BodyShop expenses)
    /// </summary>
    public Guid? TruckId { get; set; }

    /// <summary>
    ///     Filter expenses from this date
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    ///     Filter expenses until this date
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    ///     Search by vendor name or notes
    /// </summary>
    public string? Search { get; set; }
}
