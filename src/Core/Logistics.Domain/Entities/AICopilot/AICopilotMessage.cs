using System.Text.Json.Nodes;
using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// One transcript row of a copilot conversation. <see cref="ContentJson"/> is replayed to the
/// provider; <see cref="DisplayText"/> is what the UI renders.
/// </summary>
public class AICopilotMessage : Entity, ITenantEntity
{
    public Guid ConversationId { get; set; }
    public virtual AICopilotConversation Conversation { get; set; } = null!;

    /// <summary>Unique per conversation.</summary>
    public int Sequence { get; set; }

    public AICopilotMessageRole Role { get; set; }

    /// <summary>Serialized content blocks (text / tool_use / tool_result). Never exposed to clients raw.</summary>
    public string ContentJson { get; set; } = "";

    /// <summary>Text extracted from the text blocks; null for pure tool-result rows.</summary>
    public string? DisplayText { get; set; }

    /// <summary>Null only for a user message created before its turn's session exists.</summary>
    public Guid? SessionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// A plain text row in the transcript block format (<c>[{"type":"text","text":...}]</c>).
    /// The only place that shape is written outside the AI infrastructure's codec - keep both in sync.
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
