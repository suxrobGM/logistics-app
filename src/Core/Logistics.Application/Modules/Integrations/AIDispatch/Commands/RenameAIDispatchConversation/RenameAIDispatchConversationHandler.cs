using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class RenameAIDispatchConversationHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<RenameAIDispatchConversationCommand, Result>
{
    public Task<Result> Handle(RenameAIDispatchConversationCommand request, CancellationToken ct) =>
        AgentConversationCommands.RenameAsync(
            tenantUow, AgentConversationScope.Dispatch, request.ConversationId, request.Title, ct);
}
