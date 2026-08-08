using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class CancelAICopilotTurnHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser,
    IAIDispatchService dispatchService) : IAppRequestHandler<CancelAICopilotTurnCommand, Result>
{
    public Task<Result> Handle(CancelAICopilotTurnCommand request, CancellationToken ct) =>
        AgentConversationCommands.CancelTurnAsync(
            tenantUow, dispatchService, AgentConversationScope.Copilot(currentUser.GetUserId()),
            request.ConversationId, ct);
}
