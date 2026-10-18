using System.Globalization;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>
/// Rebuilds the LLM messages from the stored transcript. A pure function of the rows, so
/// consecutive turns replay identical bytes and keep hitting the prompt cache.
/// </summary>
internal static class AgentTranscriptReplay
{
    /// <summary>Rows replayed at most, give or take one turn - see <see cref="WindowStart"/>.</summary>
    internal const int MaxTranscriptMessages = 40;

    /// <summary>The window start moves in steps of this many rows, not one row per turn.</summary>
    internal const int WindowStep = 20;

    /// <summary>
    /// Longer tool results from earlier turns replay as <see cref="OmittedToolResult"/>: fleet lists
    /// are stale by then and dominate input. Errors and statuses are short and stay.
    /// </summary>
    internal const int MaxReplayedToolResultChars = 1500;

    internal const string OmittedToolResult =
        """{"omitted":"Result from an earlier turn. Call the tool again if you need it."}""";

    internal const string OmittedHistoryNote = "[Earlier conversation omitted.]";

    /// <summary>Queries the window - the <c>Messages</c> navigation would lazy-load every row.</summary>
    public static async Task<List<LlmMessage>> LoadAsync(
        ITenantUnitOfWork tenantUow, AgentConversation conversation, CancellationToken ct)
    {
        var rows = tenantUow.Repository<AgentMessage>().Query()
            .Where(m => m.ConversationId == conversation.Id);

        // Cut only at a plain chat message: starting mid-turn orphans a tool_use/tool_result pair
        // and the provider rejects the request.
        var start = WindowStart(conversation.LastSequence);
        var boundary = 1;
        if (start > 1)
        {
            boundary = await rows
                .Where(m => m.Sequence <= start && m.Role != AgentMessageRole.Assistant && m.DisplayText != null)
                .MaxAsync(m => (int?)m.Sequence, ct) ?? 1;
        }

        var window = await rows
            .AsNoTracking()
            .Where(m => m.Sequence >= boundary)
            .OrderBy(m => m.Sequence)
            .ToListAsync(ct);

        return BuildMessages(window, truncated: boundary > 1);
    }

    /// <summary>Snapped to <see cref="WindowStep"/>, so the replayed prefix holds still between steps.</summary>
    internal static int WindowStart(int lastSequence)
    {
        var overflow = lastSequence - MaxTranscriptMessages;
        return overflow <= 0 ? 1 : (overflow + WindowStep - 1) / WindowStep * WindowStep + 1;
    }

    private static List<LlmMessage> BuildMessages(List<AgentMessage> window, bool truncated)
    {
        var messages = new List<LlmMessage>(window.Count);
        foreach (var row in window)
        {
            var blocks = row.Role == AgentMessageRole.System
                ? [new LlmTextBlock($"[system note] {row.DisplayText}")]
                : AgentTranscriptCodec.Decode(row.ContentJson).Select(Compact).ToList();

            if (blocks.Count == 0)
                continue;

            if (row.Role == AgentMessageRole.User && row.DisplayText is not null)
                blocks.Insert(0, new LlmTextBlock(SentStamp(row.CreatedAt)));

            // System rows replay as user-role notes - providers only accept user/assistant mid-conversation.
            var role = row.Role == AgentMessageRole.Assistant ? LlmRole.Assistant : LlmRole.User;
            messages.Add(new LlmMessage(role, blocks));
        }

        if (truncated && messages.Count > 0 && messages[0].Role == LlmRole.User)
            messages[0].Content.Insert(0, new LlmTextBlock(OmittedHistoryNote));

        return messages;
    }

    /// <summary>From the row, never the clock, so a replayed message never changes.</summary>
    internal static string SentStamp(DateTime createdAt) =>
        $"[Sent: {createdAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)} UTC]";

    private static LlmContentBlock Compact(LlmContentBlock block) =>
        block is LlmToolResultBlock { Content.Length: > MaxReplayedToolResultChars } result
            ? result with { Content = OmittedToolResult }
            : block;
}
