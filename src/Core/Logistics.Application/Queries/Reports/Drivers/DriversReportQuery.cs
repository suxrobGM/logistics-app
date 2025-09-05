using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class DriversReportQuery : PagedIntervalQuery, IAppRequest<PagedResult<DriverReportDto>>
{
    public string? Search { get; set; }
}

