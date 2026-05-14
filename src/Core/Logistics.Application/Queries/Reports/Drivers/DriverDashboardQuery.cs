using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class DriverDashboardQuery : PagedIntervalQuery, IQuery<Result<DriverDashboardDto>>
{
}

