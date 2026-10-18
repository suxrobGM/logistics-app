using System.Text.Json;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// The tool result shells. The <c>error</c>, <c>success</c> and paging keys are named in the
/// prompts and persisted on <c>AgentDecision.ToolOutput</c>, so they live here, not per tool.
/// </summary>
internal static class ToolResult
{
    /// <summary>
    /// Return this instead of throwing: a thrown exception reaches the model only as a sanitized
    /// one-liner.
    /// </summary>
    public static string Error(string message) =>
        JsonSerializer.Serialize(new { error = message });

    public static string Ok(object payload) =>
        JsonSerializer.Serialize(payload);

    /// <summary>For tools the dispatch UI renders: a renamed key is a compile error, not a blank panel.</summary>
    public static string Typed(AgentToolResultDto result) =>
        AgentToolResultJson.Serialize(result);

    /// <summary><paramref name="successPayload"/> carries its own <c>success = true</c> and ids.</summary>
    public static string Written(IResult result, object successPayload) =>
        result.IsSuccess ? JsonSerializer.Serialize(successPayload) : WriteFailed(result);

    /// <summary>The failure arm alone, for write tools that shape their success payload later.</summary>
    public static string WriteFailed(IResult result) =>
        JsonSerializer.Serialize(new { success = false, error = result.Error });

    /// <summary>
    /// The paged envelope. <paramref name="collectionKey"/> stays per tool (<c>loads</c>,
    /// <c>invoices</c>) because the prompts name it.
    /// </summary>
    public static string Paged<T>(string collectionKey, IReadOnlyCollection<T> items, int totalItems) =>
        JsonSerializer.Serialize(new Dictionary<string, object>
        {
            [collectionKey] = items,
            ["count"] = items.Count,
            ["total"] = totalItems,
            ["truncated"] = totalItems > items.Count
        });

    /// <summary>Default page size for the paged search tools - also stated in their descriptions.</summary>
    public const int MaxResults = 20;

    /// <summary>Loads one dispatch run plans; the query's default page of 10 is too few.</summary>
    public const int MaxUnassignedLoads = 50;
}
