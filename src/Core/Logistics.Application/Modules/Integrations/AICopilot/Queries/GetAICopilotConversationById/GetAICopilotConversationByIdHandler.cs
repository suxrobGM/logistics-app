using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Queries;

internal sealed class GetAICopilotConversationByIdHandler(
    IAgentConversationQueries queries,
    ICurrentUserService currentUser)
    : IAppRequestHandler<GetAICopilotConversationByIdQuery, Result<AgentConversationDto>>
{
    public Task<Result<AgentConversationDto>> Handle(
        GetAICopilotConversationByIdQuery request, CancellationToken ct) =>
        queries.GetByIdAsync(
            AgentConversationScope.Copilot(currentUser.GetUserId()),
            request.Id, includeSessions: false, ct);
}
