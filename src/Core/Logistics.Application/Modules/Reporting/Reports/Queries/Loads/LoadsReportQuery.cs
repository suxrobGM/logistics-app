using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

public class LoadsReportQuery : DateRangeQuery, IQuery<Result<LoadsReportDto>>
{
}

