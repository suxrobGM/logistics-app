using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Dvir.Queries;

[RequiresFeature(TenantFeature.Dvir)]
public record GetPendingDvirReviewsQuery : IQuery<Result<List<DvirReportDto>>>;
