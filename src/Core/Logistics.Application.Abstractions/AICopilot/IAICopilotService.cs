namespace Logistics.Application.Abstractions.AICopilot;

/// <summary>
///     One copilot turn: the conversation to continue and the user whose permissions scope the
///     tool catalogue. Serialized as a Hangfire job argument - keep it flat.
/// </summary>
public record AICopilotTurnRequest(Guid TenantId, Guid ConversationId, Guid UserId);

/// <summary>
///     Runs copilot turns: rebuilds the conversation transcript, executes the agent loop with the
///     caller's permission scope, and persists the new messages.
/// </summary>
public interface IAICopilotService
{
    Task RunTurnAsync(AICopilotTurnRequest request, CancellationToken ct = default);
}
