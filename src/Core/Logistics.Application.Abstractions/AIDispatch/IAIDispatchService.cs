namespace Logistics.Application.Abstractions.AIDispatch;

/// <summary>
///     One dispatch turn: the tenant-shared conversation and the user who triggered it (null for a
///     background trigger). Serialized as a Hangfire job argument - keep it flat.
/// </summary>
public record AIDispatchTurnRequest(Guid TenantId, Guid ConversationId, Guid? TriggeredByUserId);

/// <summary>Runs dispatch turns through the shared agent-turn lifecycle and persists the transcript.</summary>
public interface IAIDispatchService
{
    Task RunTurnAsync(AIDispatchTurnRequest request, CancellationToken ct = default);

    /// <summary>Cancels a running session (dispatch or copilot - both share <c>AgentSession</c>).</summary>
    Task<bool> CancelAsync(Guid sessionId, CancellationToken ct = default);
}
