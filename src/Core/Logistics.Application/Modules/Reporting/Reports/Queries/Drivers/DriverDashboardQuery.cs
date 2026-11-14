using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

public class DriverDashboardQuery : PagedDateRangeQuery, IQuery<Result<DriverDashboardDto>>
{
}

