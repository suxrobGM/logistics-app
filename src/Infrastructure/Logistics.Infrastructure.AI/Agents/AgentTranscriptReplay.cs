using System.Globalization;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>
/// Rebuilds the LLM message sequence from a persisted <see cref="AgentConversation"/> transcript;
/// shared by every agent surface.
/// </summary>
/// <remarks>
/// Everything here is a pure function of the stored rows, so a turn replays the same bytes the next
/// turn will: the provider's prompt cache only hits a prefix that has not changed.
/// </remarks>
internal static class AgentTranscriptReplay
{
    /// <summary>Rows replayed at most, give or take one turn - see <see cref="WindowStart"/>.</summary>
    internal const int MaxTranscriptMessages = 40;

    /// <summary>The window start moves in steps of this many rows, not one row per turn.</summary>
    internal const int WindowStep = 20;

    /// <summary>
    /// Earlier turns' tool results longer than this replay as <see cref="OmittedToolResult"/>. Fleet
    /// lists are stale by the next turn and dominate input tokens; short results (errors, a
    /// suggestion's status) stay.
    /// </summary>
    internal const int MaxReplayedToolResultChars = 1500;

    internal const string OmittedToolResult =
        """{"omitted":"Result from an earlier turn. Call the tool again if you need it."}""";

    internal const string OmittedHistoryNote = "[Earlier conversation omitted.]";

    /// <summary>
    /// Loads the replay window in SQL - the transcript navigation would lazy-load every row and
    /// every tool result the conversation ever produced.
    /// </summary>
    public static async Task<List<LlmMessage>> LoadAsync(
        ITenantUnitOfWork tenantUow, AgentConversation conversation, CancellationToken ct)
    {
        var rows = tenantUow.Repository<AgentMessage>().Query()
            .Where(m => m.ConversationId == conversation.Id);

        // Cut only at a plain chat message: starting mid-turn orphans a tool_use/tool_result pair
        // and the provider rejects the request. The conversation always opens with one.
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

    /// <summary>
    /// The first sequence worth replaying. Snapped to <see cref="WindowStep"/> so it only moves
    /// every that many rows - a window sliding one row per turn would change the prefix every turn.
    /// </summary>
    internal static int WindowStart(int lastSequence)
    {
        var overflow = lastSequence - MaxTranscriptMessages;
        return overflow <= 0 ? 1 : (overflow + WindowStep - 1) / WindowStep * WindowStep + 1;
    }

    internal static List<LlmMessage> BuildMessages(IEnumerable<AgentMessage> window, bool truncated)
    {
        var messages = new List<LlmMessage>();
        foreach (var row in window.OrderBy(m => m.Sequence))
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

    /// <summary>
    /// When the person sent the message. Stored with the row rather than stamped per request, so
    /// replaying it never changes and the system prompt can drop "today is".
    /// </summary>
    internal static string SentStamp(DateTime createdAt) =>
        $"[Sent: {createdAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)} UTC]";

    private static LlmContentBlock Compact(LlmContentBlock block) =>
        block is LlmToolResultBlock { Content.Length: > MaxReplayedToolResultChars } result
            ? result with { Content = OmittedToolResult }
            : block;
}
