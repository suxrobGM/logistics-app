namespace Logistics.Domain.Primitives.Enums;

/// <summary>Which agent a conversation belongs to: per-user Copilot chats or tenant-shared Dispatch conversations. Queries filter on it so one agent's conversations never appear in the other.</summary>
public enum AgentConversationKind
{
    Copilot,
    Dispatch
}
