using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class RenameAICopilotConversationHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser) : IAppRequestHandler<RenameAICopilotConversationCommand, Result>
{
    public Task<Result> Handle(RenameAICopilotConversationCommand request, CancellationToken ct) =>
        AgentConversationCommands.RenameAsync(
            tenantUow, AgentConversationScope.Copilot(currentUser.GetUserId()),
            request.ConversationId, request.Title, ct);
}
