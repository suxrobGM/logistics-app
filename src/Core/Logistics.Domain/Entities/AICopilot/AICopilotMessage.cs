using System.Text.Json.Nodes;
using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// One transcript row of a copilot conversation. <see cref="ContentJson"/> holds the provider
/// content blocks (including tool_use ids) so the exact LLM message sequence can be replayed;
/// <see cref="DisplayText"/> is what the UI renders.
/// </summary>
public class AICopilotMessage : Entity, ITenantEntity
{
    public Guid ConversationId { get; set; }
    public virtual AICopilotConversation Conversation { get; set; } = null!;

    /// <summary>
    /// Position within the conversation, unique per conversation and assigned monotonically.
    /// </summary>
    public int Sequence { get; set; }

    public AICopilotMessageRole Role { get; set; }

    /// <summary>
    /// Serialized content blocks (text / tool_use / tool_result). Never exposed to clients raw.
    /// </summary>
    public string ContentJson { get; set; } = "";

    /// <summary>
    /// Human-readable text extracted from the text blocks; null for pure tool-result rows.
    /// </summary>
    public string? DisplayText { get; set; }

    /// <summary>
    /// The turn (session) this message belongs to. Null only for user messages created before
    /// their turn's session exists.
    /// </summary>
    public Guid? SessionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// A plain text row in the canonical transcript block format
    /// (<c>[{"type":"text","text":...}]</c>). The single place this shape is written outside the
    /// AI infrastructure's codec - keep the two in sync.
    /// </summary>
    public static AICopilotMessage TextMessage(
        Guid conversationId, int sequence, AICopilotMessageRole role, string text)
    {
        return new AICopilotMessage
        {
            ConversationId = conversationId,
            Sequence = sequence,
            Role = role,
            ContentJson = new JsonArray(
                new JsonObject { ["type"] = "text", ["text"] = text }).ToJsonString(),
            DisplayText = text.Length > 4000 ? text[..4000] : text
        };
    }
}
