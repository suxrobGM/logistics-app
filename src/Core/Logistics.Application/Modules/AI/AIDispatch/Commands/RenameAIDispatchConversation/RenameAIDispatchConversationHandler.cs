using Logistics.Application.Abstractions;
using Logistics.Application.Modules.AI.Agents;
using Logistics.Application.Modules.AI.Agents.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

internal sealed class RenameAIDispatchConversationHandler(
    IAgentConversationCommands commands) : IRequestHandler<RenameAIDispatchConversationCommand, Result>
{
    public Task<Result> Handle(RenameAIDispatchConversationCommand request, CancellationToken ct) =>
        commands.RenameAsync(
            AgentConversationScope.Dispatch, request.ConversationId, request.Title, ct);
}
