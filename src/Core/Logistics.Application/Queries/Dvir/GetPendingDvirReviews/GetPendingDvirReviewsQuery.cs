using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public record GetPendingDvirReviewsQuery : IAppRequest<Result<List<DvirReportDto>>>;
