using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class RenameAIDispatchConversationHandler(
    IAgentConversationCommands commands) : IAppRequestHandler<RenameAIDispatchConversationCommand, Result>
{
    public Task<Result> Handle(RenameAIDispatchConversationCommand request, CancellationToken ct) =>
        commands.RenameAsync(
            AgentConversationScope.Dispatch, request.ConversationId, request.Title, ct);
}
