namespace Logistics.Application.Abstractions.Agents;

/// <summary>
/// Where a tool may be called. Opt-in, and the default is the narrowest surface: a tool nobody
/// widened stays on the copilot, where a named user with checked permissions is driving it. The two
/// unattended surfaces are unattended in different ways, so each is named at the declaration site.
/// </summary>
[Flags]
public enum AgentSurfaces
{
    /// <summary>One user's copilot turn, scoped to that caller's permissions.</summary>
    Copilot = 1,

    /// <summary>
    /// The fleet dispatch run. No caller permissions to scope by - the endpoint's policy stands in -
    /// and a write becomes a suggestion awaiting dispatcher approval.
    /// </summary>
    Dispatch = 2,

    /// <summary>
    /// An MCP client holding an API key. There is no approval step and no person to attribute a
    /// write to, so a tool that needs either must leave this off.
    /// </summary>
    Mcp = 4,

    /// <summary>Every surface. For a read that any caller may run.</summary>
    All = Copilot | Dispatch | Mcp
}
