using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

/// <summary>
/// Query to generate a payroll report for a specified date range.
/// </summary>
public class PayrollReportQuery : PagedIntervalQuery, IQuery<Result<PayrollReportDto>>
{
}
