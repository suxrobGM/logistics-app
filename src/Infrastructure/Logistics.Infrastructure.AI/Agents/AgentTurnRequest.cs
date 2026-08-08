namespace Logistics.Infrastructure.AI.Agents;

/// <summary>
/// One agent turn, kind-agnostic: the conversation and the tenant/user it runs for. Every surface
/// (copilot today, dispatch from Phase 3) maps its own port-level request onto this before calling
/// <see cref="AgentTurnService"/>.
/// </summary>
internal sealed record AgentTurnRequest(Guid TenantId, Guid ConversationId, Guid? TriggeredByUserId);
