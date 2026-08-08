using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Agents.Dispatch;

/// <summary>
/// Adapter from the dispatch port to <see cref="AgentTurnService"/> + <see cref="DispatchAgentSurface"/>,
/// like <c>AICopilotService</c>. Cancellation stays here, not in the surface - both session types
/// cancel through the same registry.
/// </summary>
internal sealed class AIDispatchService(
    AgentTurnService turnService,
    DispatchAgentSurface surface,
    AgentSessionCancellationRegistry cancellationRegistry,
    ITenantUnitOfWork tenantUow) : IAIDispatchService
{
    public Task RunTurnAsync(AIDispatchTurnRequest request, CancellationToken ct = default) =>
        turnService.RunTurnAsync(
            new AgentTurnRequest(request.TenantId, request.ConversationId, request.TriggeredByUserId), surface, ct);

    public async Task<bool> CancelAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await tenantUow.Repository<AgentSession>().GetByIdAsync(sessionId, ct);
        if (session is null || session.Status != AgentSessionStatus.Running)
            return false;

        cancellationRegistry.TryCancel(sessionId);
        session.Cancel();
        await tenantUow.SaveChangesAsync(ct);
        return true;
    }
}
