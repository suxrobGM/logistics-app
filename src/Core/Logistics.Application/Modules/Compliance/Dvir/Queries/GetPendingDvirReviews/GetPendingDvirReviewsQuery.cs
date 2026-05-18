using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Dvir.Queries;

public record GetPendingDvirReviewsQuery : IQuery<Result<List<DvirReportDto>>>;
