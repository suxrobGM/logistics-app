using Logistics.Application.Abstractions;
using Logistics.Application.Modules.AI.Agents;
using Logistics.Application.Modules.AI.Agents.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AIDispatch.Queries;

internal sealed class GetAIDispatchConversationByIdHandler(
    IAgentConversationQueries queries)
    : IRequestHandler<GetAIDispatchConversationByIdQuery, Result<AgentConversationDto>>
{
    public Task<Result<AgentConversationDto>> Handle(
        GetAIDispatchConversationByIdQuery request, CancellationToken ct) =>
        queries.GetByIdAsync(
            AgentConversationScope.Dispatch, request.Id, includeSessions: true, ct);
}
