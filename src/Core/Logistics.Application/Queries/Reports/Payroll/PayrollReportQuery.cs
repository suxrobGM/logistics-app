using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

/// <summary>
/// Query to generate a payroll report for a specified date range.
/// </summary>
public class PayrollReportQuery : PagedIntervalQuery, IAppRequest<Result<PayrollReportDto>>
{
}
