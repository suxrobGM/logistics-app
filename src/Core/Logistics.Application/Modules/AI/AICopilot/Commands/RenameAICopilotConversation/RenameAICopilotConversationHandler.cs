using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Modules.AI.Agents;
using Logistics.Application.Modules.AI.Agents.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AICopilot.Commands;

internal sealed class RenameAICopilotConversationHandler(
    IAgentConversationCommands commands,
    ICurrentUserService currentUser) : IRequestHandler<RenameAICopilotConversationCommand, Result>
{
    public Task<Result> Handle(RenameAICopilotConversationCommand request, CancellationToken ct) =>
        commands.RenameAsync(
            AgentConversationScope.Copilot(currentUser.GetUserId()),
            request.ConversationId, request.Title, ct);
}
