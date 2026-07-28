using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
///     Progress of an in-flight copilot turn, pushed over SignalR while the agent works.
/// </summary>
public record AICopilotTurnUpdateDto
{
    public Guid ConversationId { get; set; }
    public Guid SessionId { get; set; }
    public AIDispatchSessionStatus Status { get; set; }
    public int TotalTokensUsed { get; set; }
    public int DecisionCount { get; set; }
    public string? ErrorMessage { get; set; }
}
