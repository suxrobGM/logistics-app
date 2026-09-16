using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Reports.Queries;

public class DriverDashboardQuery : PagedIntervalQuery, IQuery<Result<DriverDashboardDto>>
{
}

