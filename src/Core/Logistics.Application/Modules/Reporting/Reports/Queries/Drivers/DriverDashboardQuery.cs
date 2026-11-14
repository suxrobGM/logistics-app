using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

public class DriverDashboardQuery : DateRangeQuery, IQuery<Result<DriverDashboardDto>>
{
}

