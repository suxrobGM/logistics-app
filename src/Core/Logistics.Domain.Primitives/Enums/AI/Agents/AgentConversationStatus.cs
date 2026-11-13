namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Concurrency guard for an agent conversation: Running while a turn is in flight, Idle otherwise.
/// </summary>
public enum AgentConversationStatus
{
    Idle,
    Running
}
