using System.Text.Json;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// The wire format every tool returns to the model. The <c>error</c>, <c>success</c>, and paging
/// keys are named in both system prompts and persisted on <c>AgentDecision.ToolOutput</c>, so
/// they are a contract - keep them in one place rather than retyping the shell per tool.
/// </summary>
internal static class ToolResult
{
    /// <summary>
    /// A failure the model should read and react to. Tools return this instead of throwing: the
    /// agent loop catches exceptions, but the agent loses all context on what went wrong.
    /// </summary>
    public static string Error(string message) =>
        JsonSerializer.Serialize(new { error = message });

    /// <summary>Serializes a payload as-is.</summary>
    public static string Ok(object payload) =>
        JsonSerializer.Serialize(payload);

    /// <summary>
    /// The declared result shape, for the tools whose output the dispatch UI renders. The same type
    /// reaches the client, so renaming a key for the model is a compile error, not a blank panel.
    /// </summary>
    public static string Typed(AgentToolResultDto result) =>
        AgentToolResultJson.Serialize(result);

    /// <summary>
    /// Result of a write tool. <paramref name="successPayload"/> carries its own
    /// <c>success = true</c> plus whatever identifiers reference the mutated entity; the failure
    /// arm is uniform, which is the half worth sharing.
    /// </summary>
    public static string Written(IResult result, object successPayload) =>
        result.IsSuccess ? JsonSerializer.Serialize(successPayload) : WriteFailed(result);

    /// <summary>
    /// The failure arm on its own, for write tools that bail early because they still have work to
    /// do (reading the created entity back, say) before they can shape the success payload.
    /// </summary>
    public static string WriteFailed(IResult result) =>
        JsonSerializer.Serialize(new { success = false, error = result.Error });

    /// <summary>
    /// The <c>{ items, count, total, truncated }</c> envelope shared by the paged search tools.
    /// <paramref name="collectionKey"/> stays per-tool (<c>loads</c>, <c>invoices</c>, ...) because
    /// the prompts refer to those names.
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
}
