using Logistics.Application.Abstractions;
using Logistics.Application.Modules.AI.Agents;
using Logistics.Application.Modules.AI.Agents.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

internal sealed class CancelAIDispatchTurnHandler(
    IAgentConversationCommands commands) : IRequestHandler<CancelAIDispatchTurnCommand, Result>
{
    public Task<Result> Handle(CancelAIDispatchTurnCommand request, CancellationToken ct) =>
        commands.CancelTurnAsync(AgentConversationScope.Dispatch, request.ConversationId, ct);
}
