using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Queries;

public class GetDriverHosLogsQuery : PagedIntervalQuery, IQuery<PagedResult<HosLogDto>>
{
    public Guid EmployeeId { get; set; }
}
