using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.Application.Abstractions.AIDispatch;

/// <summary>
/// Core orchestration service for the AI dispatch agent.
/// Manages the agent loop: context gathering → Claude reasoning → tool execution → response.
/// </summary>
public interface IAIDispatchService
{
    Task<AgentSession> RunAsync(AIDispatchRequest request, CancellationToken ct = default);
    Task<bool> CancelAsync(Guid sessionId, CancellationToken ct = default);
}

public record AIDispatchRequest(
    Guid TenantId,
    AgentAutonomyMode Mode,
    Guid? TriggeredByUserId,
    string? Instructions = null,
    string? RejectionContext = null);
