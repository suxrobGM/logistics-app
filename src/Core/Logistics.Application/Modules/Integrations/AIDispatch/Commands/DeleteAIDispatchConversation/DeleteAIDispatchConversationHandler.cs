using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class DeleteAIDispatchConversationHandler(
    IAgentConversationCommands commands) : IAppRequestHandler<DeleteAIDispatchConversationCommand, Result>
{
    public Task<Result> Handle(DeleteAIDispatchConversationCommand request, CancellationToken ct) =>
        commands.DeleteAsync(AgentConversationScope.Dispatch, request.ConversationId, ct);
}
