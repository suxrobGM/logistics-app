namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// A fleet-wide dispatch run or a single copilot turn. Both share the session's quota, token
/// accounting, and decisions.
/// </summary>
public enum AIDispatchSessionType
{
    Dispatch,
    Copilot
}
