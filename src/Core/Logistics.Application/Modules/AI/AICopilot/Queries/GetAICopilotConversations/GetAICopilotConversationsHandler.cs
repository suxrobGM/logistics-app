using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Modules.AI.Agents;
using Logistics.Application.Modules.AI.Agents.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AICopilot.Queries;

internal sealed class GetAICopilotConversationsHandler(
    IAgentConversationQueries queries,
    ICurrentUserService currentUser)
    : IRequestHandler<GetAICopilotConversationsQuery, PagedResult<AgentConversationDto>>
{
    public Task<PagedResult<AgentConversationDto>> Handle(
        GetAICopilotConversationsQuery request, CancellationToken ct) =>
        queries.ListAsync(
            AgentConversationScope.Copilot(currentUser.GetUserId()),
            request.Page, request.PageSize, ct);
}
