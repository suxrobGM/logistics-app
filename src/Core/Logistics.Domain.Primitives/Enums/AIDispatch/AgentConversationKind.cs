namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Which surface owns an <see cref="Logistics.Domain.Entities.AgentConversation"/>: the per-user
/// TMS copilot today, the tenant-shared dispatch conversation from Phase 3 on. Every copilot
/// query/list/get filters on <see cref="Copilot"/> so a future dispatch conversation never leaks
/// into the copilot surface.
/// </summary>
public enum AgentConversationKind
{
    Copilot,
    Dispatch
}
