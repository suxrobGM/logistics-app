using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class DeleteAICopilotConversationHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser) : IAppRequestHandler<DeleteAICopilotConversationCommand, Result>
{
    public Task<Result> Handle(DeleteAICopilotConversationCommand request, CancellationToken ct) =>
        AgentConversationCommands.DeleteAsync(
            tenantUow, AgentConversationScope.Copilot(currentUser.GetUserId()), request.ConversationId, ct);
}
