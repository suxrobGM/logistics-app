namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Which surface owns an <see cref="Logistics.Domain.Entities.AgentConversation"/>: per-user
/// copilot or tenant-shared dispatch. Every handler must filter on its own kind - a conversation
/// must never leak into the other surface.
/// </summary>
public enum AgentConversationKind
{
    Copilot,
    Dispatch
}
