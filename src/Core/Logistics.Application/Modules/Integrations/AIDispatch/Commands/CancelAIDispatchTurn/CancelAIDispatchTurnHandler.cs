using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class CancelAIDispatchTurnHandler(
    ITenantUnitOfWork tenantUow,
    IAIDispatchService dispatchService) : IAppRequestHandler<CancelAIDispatchTurnCommand, Result>
{
    public Task<Result> Handle(CancelAIDispatchTurnCommand request, CancellationToken ct) =>
        AgentConversationCommands.CancelTurnAsync(
            tenantUow, dispatchService, AgentConversationScope.Dispatch, request.ConversationId, ct);
}
