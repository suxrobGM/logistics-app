using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

internal sealed class GetAIDispatchConversationByIdHandler(
    IAgentConversationQueries queries)
    : IAppRequestHandler<GetAIDispatchConversationByIdQuery, Result<AgentConversationDto>>
{
    public Task<Result<AgentConversationDto>> Handle(
        GetAIDispatchConversationByIdQuery request, CancellationToken ct) =>
        queries.GetByIdAsync(
            AgentConversationScope.Dispatch, request.Id, includeSessions: true, ct);
}
