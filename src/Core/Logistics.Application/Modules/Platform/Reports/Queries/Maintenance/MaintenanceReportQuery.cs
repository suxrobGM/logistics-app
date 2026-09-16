using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Reports.Queries;

public class MaintenanceReportQuery : PagedIntervalQuery, IQuery<Result<MaintenanceReportDto>>
{
}
