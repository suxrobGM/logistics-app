using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Agents.Dispatch;

/// <summary>
/// Thin adapter from the dispatch port to the shared turn lifecycle: maps the dispatch turn request
/// onto <see cref="AgentTurnRequest"/> and runs it through <see cref="AgentTurnService"/> with
/// <see cref="DispatchAgentSurface"/>, exactly like <c>AICopilotService</c>. Cancellation is generic
/// across session types (dispatch and copilot both cancel through the same registry), so it stays
/// here rather than moving into the surface.
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
