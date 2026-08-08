using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Queries;

internal sealed class GetAICopilotConversationByIdHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser)
    : IAppRequestHandler<GetAICopilotConversationByIdQuery, Result<AgentConversationDto>>
{
    public Task<Result<AgentConversationDto>> Handle(
        GetAICopilotConversationByIdQuery request, CancellationToken ct) =>
        AgentConversationQueries.GetByIdAsync(
            tenantUow, AgentConversationScope.Copilot(currentUser.GetUserId()),
            request.Id, includeSessions: false, ct);
}
