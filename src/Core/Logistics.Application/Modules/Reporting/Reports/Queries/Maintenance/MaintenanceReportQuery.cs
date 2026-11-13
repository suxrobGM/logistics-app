using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

public class MaintenanceReportQuery : PagedIntervalQuery, IQuery<Result<MaintenanceReportDto>>
{
}
