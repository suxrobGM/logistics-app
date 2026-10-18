using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// The hazmat class the agent sees: null on non-hazmat loads, "unspecified" on a hazmat load without a
/// class, so the agent never reads a missing class as "not hazmat".
/// </summary>
internal static class AgentHazmat
{
    public static string? DescribeClass(LoadDto load) =>
        load.IsHazmat ? load.HazmatClass?.ToString() ?? "unspecified" : null;
}
