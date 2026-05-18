using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Queries;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public sealed class GetPendingDecisionsQuery : IQuery<Result<List<AiDispatchDecisionDto>>>
{
}
