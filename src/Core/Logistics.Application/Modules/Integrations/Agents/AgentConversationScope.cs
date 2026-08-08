using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.Agents;

/// <summary>
/// Which conversations one agent surface may act on. Dispatch conversations are tenant-shared, so
/// only the kind is checked and the endpoint policy gates the caller; copilot conversations are
/// private to their creator, and an unauthenticated caller matches none of them.
/// </summary>
internal sealed record AgentConversationScope(
    AgentConversationKind Kind,
    bool RequireOwner,
    Guid? CallerId)
{
    public static AgentConversationScope Dispatch { get; } =
        new(AgentConversationKind.Dispatch, RequireOwner: false, CallerId: null);

    public static AgentConversationScope Copilot(Guid? callerId) =>
        new(AgentConversationKind.Copilot, RequireOwner: true, callerId);
}
