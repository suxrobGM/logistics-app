namespace Logistics.Application.Abstractions.AICopilot;

/// <summary>
///     One copilot turn: the conversation, the user whose permissions scope the tool catalogue,
///     and the TMS route they sent from. Serialized as a Hangfire job argument - keep it flat.
/// </summary>
public record AICopilotTurnRequest(Guid TenantId, Guid ConversationId, Guid UserId, string? PageContext = null);

/// <summary>Runs copilot turns with the caller's permission scope and persists the transcript.</summary>
public interface IAICopilotService
{
    Task RunTurnAsync(AICopilotTurnRequest request, CancellationToken ct = default);
}
