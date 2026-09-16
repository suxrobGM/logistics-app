using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Queries;

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
