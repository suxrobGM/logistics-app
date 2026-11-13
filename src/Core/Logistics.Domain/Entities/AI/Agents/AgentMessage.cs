using System.Text.Json.Nodes;
using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>A message in an agent conversation. <see cref="ContentJson"/> is the provider-format content replayed to the LLM; <see cref="DisplayText"/> is what the UI shows.</summary>
public class AgentMessage : Entity, ITenantEntity
{
    public Guid ConversationId { get; set; }
    public virtual AgentConversation Conversation { get; set; } = null!;

    /// <summary>Unique per conversation.</summary>
    public int Sequence { get; set; }

    public AgentMessageRole Role { get; set; }

    /// <summary>Serialized content blocks (text / tool_use / tool_result). Never exposed to clients raw.</summary>
    public string ContentJson { get; set; } = "";

    /// <summary>Text extracted from the text blocks; null for pure tool-result rows.</summary>
    public string? DisplayText { get; set; }

    /// <summary>Null only for a user message created before its turn's session exists.</summary>
    public Guid? SessionId { get; set; }

    /// <summary>User who sent the message; null for agent messages, app-generated notices no user triggered, and inbound broker replies.</summary>
    public Guid? SentByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Builds a plain-text message. AgentTranscriptCodec writes the same ContentJson shape, so the two must match.</summary>
    public static AgentMessage TextMessage(
        Guid conversationId, int sequence, AgentMessageRole role, string text, Guid? sentByUserId = null)
    {
        return new AgentMessage
        {
            ConversationId = conversationId,
            Sequence = sequence,
            Role = role,
            SentByUserId = sentByUserId,
            ContentJson = new JsonArray(
                new JsonObject { ["type"] = "text", ["text"] = text }).ToJsonString(),
            DisplayText = text.Length > 4000 ? text[..4000] : text
        };
    }
}
