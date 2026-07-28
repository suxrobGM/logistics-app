namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Concurrency guard for a copilot conversation: Running while a turn is in flight, Idle otherwise.
/// </summary>
public enum AICopilotConversationStatus
{
    Idle,
    Running
}
