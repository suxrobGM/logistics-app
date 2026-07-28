namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Discriminates what kind of agent run a session records: a fleet-wide dispatch run or a single
/// conversational copilot turn. Both share the session's quota, token accounting, and decisions.
/// </summary>
public enum AIDispatchSessionType
{
    Dispatch,
    Copilot
}
