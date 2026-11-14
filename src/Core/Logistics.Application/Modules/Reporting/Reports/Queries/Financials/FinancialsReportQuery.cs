using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

public class FinancialsReportQuery : DateRangeQuery, IQuery<Result<FinancialsReportDto>>
{
}

