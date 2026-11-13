using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AIDispatch.Queries;

[RequiresFeature(TenantFeature.AIDispatch)]
public class GetAIDispatchConversationByIdQuery : IQuery<Result<AgentConversationDto>>, IHaveId
{
    public Guid Id { get; set; }
}
