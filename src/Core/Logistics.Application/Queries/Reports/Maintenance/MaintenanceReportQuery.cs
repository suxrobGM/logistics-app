using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class MaintenanceReportQuery : PagedIntervalQuery, IAppRequest<Result<MaintenanceReportDto>>
{
}
