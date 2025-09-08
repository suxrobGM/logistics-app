using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class FinancialsReportQuery : PagedIntervalQuery, IAppRequest<Result<FinancialsReportDto>>
{
}

