using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Modules.AI.Agents;
using Logistics.Application.Modules.AI.Agents.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AICopilot.Commands;

internal sealed class DeleteAICopilotConversationHandler(
    IAgentConversationCommands commands,
    ICurrentUserService currentUser) : IRequestHandler<DeleteAICopilotConversationCommand, Result>
{
    public Task<Result> Handle(DeleteAICopilotConversationCommand request, CancellationToken ct) =>
        commands.DeleteAsync(
            AgentConversationScope.Copilot(currentUser.GetUserId()), request.ConversationId, ct);
}
