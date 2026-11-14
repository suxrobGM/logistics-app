using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

public class FinancialsReportQuery : PagedDateRangeQuery, IQuery<Result<FinancialsReportDto>>
{
}

