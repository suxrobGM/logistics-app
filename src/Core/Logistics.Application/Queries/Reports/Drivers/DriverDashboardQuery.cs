using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class DriverDashboardQuery : PagedIntervalQuery, IAppRequest<Result<DriverDashboardDto>>
{
}

