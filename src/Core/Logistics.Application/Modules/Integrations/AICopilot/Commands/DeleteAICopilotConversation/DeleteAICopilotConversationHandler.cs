using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class DeleteAICopilotConversationHandler(
    IAgentConversationCommands commands,
    ICurrentUserService currentUser) : IRequestHandler<DeleteAICopilotConversationCommand, Result>
{
    public Task<Result> Handle(DeleteAICopilotConversationCommand request, CancellationToken ct) =>
        commands.DeleteAsync(
            AgentConversationScope.Copilot(currentUser.GetUserId()), request.ConversationId, ct);
}
