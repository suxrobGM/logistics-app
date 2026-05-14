using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public record GetPendingDvirReviewsQuery : IQuery<Result<List<DvirReportDto>>>;
