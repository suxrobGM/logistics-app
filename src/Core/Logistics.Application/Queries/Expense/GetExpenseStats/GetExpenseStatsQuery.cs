using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetExpenseStatsQuery : IAppRequest<Result<ExpenseStatsDto>>
{
    /// <summary>
    ///     Filter from this date
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    ///     Filter until this date
    /// </summary>
    public DateTime? ToDate { get; set; }
}
