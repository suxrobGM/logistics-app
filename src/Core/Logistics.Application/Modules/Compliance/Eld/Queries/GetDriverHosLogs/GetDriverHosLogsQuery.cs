using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Queries;

public class GetDriverHosLogsQuery : PagedDateRangeQuery, IQuery<PagedResult<HosLogDto>>
{
    public Guid EmployeeId { get; set; }
}
