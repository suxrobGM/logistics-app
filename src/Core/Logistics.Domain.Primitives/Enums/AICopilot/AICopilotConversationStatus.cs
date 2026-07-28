namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Concurrency guard for a copilot conversation: Running while a turn is in flight, Idle otherwise.
/// "Awaiting user input" is simply Idle - a turn always terminates.
/// </summary>
public enum AICopilotConversationStatus
{
    Idle,
    Running
}
