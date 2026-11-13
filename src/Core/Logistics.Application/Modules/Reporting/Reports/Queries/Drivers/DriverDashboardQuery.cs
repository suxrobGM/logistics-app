using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

public class DriverDashboardQuery : PagedIntervalQuery, IQuery<Result<DriverDashboardDto>>
{
}

