using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class LoadsReportQuery : PagedIntervalQuery, IAppRequest<Result<LoadsReportDto>>
{
}

