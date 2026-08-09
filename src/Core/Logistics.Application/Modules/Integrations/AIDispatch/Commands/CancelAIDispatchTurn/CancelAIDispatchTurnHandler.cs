using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class CancelAIDispatchTurnHandler(
    IAgentConversationCommands commands) : IAppRequestHandler<CancelAIDispatchTurnCommand, Result>
{
    public Task<Result> Handle(CancelAIDispatchTurnCommand request, CancellationToken ct) =>
        commands.CancelTurnAsync(AgentConversationScope.Dispatch, request.ConversationId, ct);
}
