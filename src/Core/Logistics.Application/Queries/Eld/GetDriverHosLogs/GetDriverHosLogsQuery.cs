using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetDriverHosLogsQuery : PagedIntervalQuery, IAppRequest<PagedResult<HosLogDto>>
{
    public Guid EmployeeId { get; set; }
}
