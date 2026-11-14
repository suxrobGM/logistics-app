using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

public class SafetyReportQuery : DateRangeQuery, IQuery<Result<SafetyReportDto>>
{
}
