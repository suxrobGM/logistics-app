using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class RenameAICopilotConversationHandler(
    IAgentConversationCommands commands,
    ICurrentUserService currentUser) : IAppRequestHandler<RenameAICopilotConversationCommand, Result>
{
    public Task<Result> Handle(RenameAICopilotConversationCommand request, CancellationToken ct) =>
        commands.RenameAsync(
            AgentConversationScope.Copilot(currentUser.GetUserId()),
            request.ConversationId, request.Title, ct);
}
