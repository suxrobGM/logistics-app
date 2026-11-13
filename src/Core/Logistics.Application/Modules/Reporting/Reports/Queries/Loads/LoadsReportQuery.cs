using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

public class LoadsReportQuery : PagedIntervalQuery, IQuery<Result<LoadsReportDto>>
{
}

