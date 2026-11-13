using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

public class DriversReportQuery : PagedIntervalQuery, IQuery<PagedResult<DriverReportDto>>
{
    public string? Search { get; set; }
}

