namespace Logistics.Application.Abstractions.AICopilot;

/// <summary>
///     One copilot turn: the conversation and the user whose permissions scope the tool catalogue.
///     Serialized as a Hangfire job argument - keep it flat.
/// </summary>
public record AICopilotTurnRequest(Guid TenantId, Guid ConversationId, Guid UserId);

/// <summary>Runs copilot turns with the caller's permission scope and persists the transcript.</summary>
public interface IAICopilotService
{
    Task RunTurnAsync(AICopilotTurnRequest request, CancellationToken ct = default);
}
