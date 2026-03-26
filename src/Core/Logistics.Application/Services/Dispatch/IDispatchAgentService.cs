using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Services;

/// <summary>
/// Core orchestration service for the AI dispatch agent.
/// Manages the agent loop: context gathering → Claude reasoning → tool execution → response.
/// </summary>
public interface IDispatchAgentService
{
    Task<DispatchSession> RunAsync(DispatchAgentRequest request, CancellationToken ct = default);
    Task<bool> CancelAsync(Guid sessionId, CancellationToken ct = default);
}

public record DispatchAgentRequest(
    Guid TenantId,
    DispatchAgentMode Mode,
    Guid? TriggeredByUserId,
    bool IsOverage = false);
