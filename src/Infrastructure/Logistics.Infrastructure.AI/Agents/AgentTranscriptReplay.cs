using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>
/// Rebuilds the LLM message sequence from a persisted transcript, shared by every agent surface
/// that replays an <see cref="AgentConversation"/> (copilot today, dispatch from Phase 3).
/// </summary>
internal static class AgentTranscriptReplay
{
    private const int MaxTranscriptMessages = 30;

    public static List<LlmMessage> BuildMessages(IEnumerable<AgentMessage> transcript)
    {
        var ordered = transcript.OrderBy(m => m.Sequence).ToList();
        var (window, truncated) = TakeRecentWindow(ordered);

        var messages = new List<LlmMessage>();
        foreach (var row in window)
        {
            var blocks = row.Role == AgentMessageRole.System
                ? [new LlmTextBlock($"[system note] {row.DisplayText}")]
                : AgentTranscriptCodec.Decode(row.ContentJson);

            if (blocks.Count == 0)
                continue;

            // System rows replay as user-role notes - providers only accept user/assistant mid-conversation.
            var role = row.Role == AgentMessageRole.Assistant ? LlmRole.Assistant : LlmRole.User;
            messages.Add(new LlmMessage(role, blocks));
        }

        if (truncated && messages.Count > 0 && messages[0].Role == LlmRole.User)
            messages[0].Content.Insert(0, new LlmTextBlock("[Earlier conversation omitted.]"));

        return messages;
    }

    /// <summary>
    /// Cuts only at a plain user chat message: starting the window mid-turn orphans a
    /// tool_use/tool_result pair and the provider rejects the request.
    /// </summary>
    private static (List<AgentMessage> Window, bool Truncated) TakeRecentWindow(List<AgentMessage> ordered)
    {
        if (ordered.Count <= MaxTranscriptMessages)
            return (ordered, false);

        var start = ordered.Count - MaxTranscriptMessages;
        while (start < ordered.Count && !IsChatBoundary(ordered[start]))
            start++;

        // No boundary in the window - replay everything rather than corrupt it.
        return start >= ordered.Count ? (ordered, false) : (ordered[start..], start > 0);
    }

    private static bool IsChatBoundary(AgentMessage row) =>
        row.Role != AgentMessageRole.Assistant && row.DisplayText is not null;
}
