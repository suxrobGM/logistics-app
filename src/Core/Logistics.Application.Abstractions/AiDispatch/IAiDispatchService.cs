using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Application.Abstractions.AiDispatch;

namespace Logistics.Application.Abstractions.AiDispatch;

/// <summary>
/// Core orchestration service for the AI dispatch agent.
/// Manages the agent loop: context gathering → Claude reasoning → tool execution → response.
/// </summary>
public interface IAiDispatchService
{
    Task<AiDispatchSession> RunAsync(AiDispatchRequest request, CancellationToken ct = default);
    Task<bool> CancelAsync(Guid sessionId, CancellationToken ct = default);
}

public record AiDispatchRequest(
    Guid TenantId,
    AiDispatchMode Mode,
    Guid? TriggeredByUserId,
    bool IsOverage = false,
    string? Instructions = null,
    string? RejectionContext = null);
