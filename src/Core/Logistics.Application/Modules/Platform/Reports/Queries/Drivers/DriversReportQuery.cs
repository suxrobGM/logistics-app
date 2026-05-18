using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Reports.Queries;

public class DriversReportQuery : PagedIntervalQuery, IQuery<PagedResult<DriverReportDto>>
{
    public string? Search { get; set; }
}

