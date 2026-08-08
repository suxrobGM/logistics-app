using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class DeleteAIDispatchConversationHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<DeleteAIDispatchConversationCommand, Result>
{
    public Task<Result> Handle(DeleteAIDispatchConversationCommand request, CancellationToken ct) =>
        AgentConversationCommands.DeleteAsync(
            tenantUow, AgentConversationScope.Dispatch, request.ConversationId, ct);
}
