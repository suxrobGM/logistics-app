using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>A chat thread with an AI agent. <see cref="Kind"/> distinguishes per-user Copilot chats from tenant-shared Dispatch conversations.</summary>
public class AgentConversation : AuditableEntity, ITenantEntity
{
    /// <summary>A Running turn older than this is assumed crashed and may be taken over.</summary>
    public static readonly TimeSpan StaleTurnWindow = TimeSpan.FromMinutes(15);

    /// <summary>User who started the conversation. For Copilot only this user may read or post to it; for Dispatch, which the whole tenant shares, it is recorded for audit only.</summary>
    public Guid CreatedById { get; init; }

    public AgentConversationKind Kind { get; init; } = AgentConversationKind.Copilot;

    /// <summary>Derived from the first user message.</summary>
    public string? Title { get; set; }

    public AgentConversationStatus Status { get; private set; } = AgentConversationStatus.Idle;

    /// <summary>Lets a crashed turn be taken over after a staleness window.</summary>
    public DateTime? TurnStartedAt { get; private set; }

    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public virtual List<AgentMessage> Messages { get; } = [];

    /// <summary>Creates a text message for this conversation. The caller reserves <paramref name="sequence"/> in the database first and adds the returned message with the repository.</summary>
    public AgentMessage AddTextMessage(int sequence, AgentMessageRole role, string text, Guid? sentByUserId = null)
    {
        var message = AgentMessage.TextMessage(Id, sequence, role, text, sentByUserId);
        LastMessageAt = DateTime.UtcNow;
        return message;
    }

    /// <summary>Creates a message holding provider-format turn content, with the same sequence and repository rules as <see cref="AddTextMessage"/>.</summary>
    public AgentMessage AddTurnMessage(
        int sequence, Guid sessionId, AgentMessageRole role, string contentJson, string? displayText)
    {
        return new AgentMessage
        {
            ConversationId = Id,
            Sequence = sequence,
            Role = role,
            ContentJson = contentJson,
            DisplayText = displayText,
            SessionId = sessionId
        };
    }

    public void BeginTurn()
    {
        Status = AgentConversationStatus.Running;
        TurnStartedAt = DateTime.UtcNow;
    }

    public void EndTurn()
    {
        Status = AgentConversationStatus.Idle;
        TurnStartedAt = null;
    }
}
