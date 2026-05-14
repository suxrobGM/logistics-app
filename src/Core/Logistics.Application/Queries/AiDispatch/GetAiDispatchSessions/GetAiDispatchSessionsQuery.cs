using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public sealed class GetAiDispatchSessionsQuery : PagedQuery, IQuery<PagedResult<AiDispatchSessionDto>>
{
    public AiDispatchSessionStatus? Status { get; set; }
}
