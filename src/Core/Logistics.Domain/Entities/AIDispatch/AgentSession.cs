using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents a single AI dispatch agent run.
/// Tracks the agent's status, token usage, and all decisions made.
/// </summary>
public class AgentSession : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// Sequential number of the session, unique within the tenant.
    /// </summary>
    public long Number { get; private set; }

    public AgentSessionType Type { get; init; } = AgentSessionType.Dispatch;
    public AgentSessionStatus Status { get; private set; } = AgentSessionStatus.Running;

    /// <summary>
    /// The conversation this turn belongs to. Null only for legacy sessions created before
    /// conversations existed.
    /// </summary>
    public Guid? ConversationId { get; init; }
    public virtual AgentConversation? Conversation { get; init; }

    /// <summary>
    /// The user who triggered this session. Null if triggered by background job.
    /// </summary>
    public Guid? TriggeredByUserId { get; init; }

    public DateTime StartedAt { get; init; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Total LLM tokens consumed during this session.
    /// </summary>
    public int TotalTokensUsed => InputTokensUsed + OutputTokensUsed;

    /// <summary>
    /// Input tokens consumed during this session.
    /// </summary>
    public int InputTokensUsed { get; set; }

    /// <summary>
    /// Output tokens consumed during this session.
    /// </summary>
    public int OutputTokensUsed { get; set; }

    /// <summary>
    /// Cached input tokens read from prompt cache.
    /// </summary>
    public int CacheReadTokens { get; set; }

    /// <summary>
    /// Tokens written to prompt cache.
    /// </summary>
    public int CacheCreationTokens { get; set; }

    /// <summary>
    /// Estimated cost in USD for this session.
    /// </summary>
    public decimal EstimatedCostUsd { get; set; }

    /// <summary>
    /// The model used for the turn.
    /// </summary>
    public string? ModelUsed { get; set; }

    /// <summary>
    /// Number of decisions made by the agent.
    /// </summary>
    public int DecisionCount { get; set; }

    /// <summary>
    /// Agent's summary of the session outcome.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Error message if the session failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether this session exceeded the tenant's weekly AI request quota.
    /// Overage sessions are billed at the plan's overage rate.
    /// </summary>
    public bool IsOverage { get; set; }

    public virtual List<AgentDecision> Decisions { get; } = [];

    public void Complete(string? summary = null)
    {
        Status = AgentSessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Summary = summary;
    }

    public void Fail(string errorMessage)
    {
        Status = AgentSessionStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }

    public void Cancel()
    {
        Status = AgentSessionStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }
}
