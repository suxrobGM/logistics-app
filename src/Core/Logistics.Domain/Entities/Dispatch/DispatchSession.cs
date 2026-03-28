using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents a single AI dispatch agent run.
/// Tracks the agent's mode, status, token usage, and all decisions made.
/// </summary>
public class DispatchSession : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// Sequential number of the session, unique within the tenant.
    /// </summary>
    public long Number { get; private set; }

    public DispatchAgentMode Mode { get; init; }
    public DispatchSessionStatus Status { get; private set; } = DispatchSessionStatus.Running;

    /// <summary>
    /// The user who triggered this session. Null if triggered by background job.
    /// </summary>
    public Guid? TriggeredByUserId { get; init; }

    public DateTime StartedAt { get; init; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// User-provided instructions for this session.
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// Total Claude API tokens consumed during this session.
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
    /// The Claude model used for this session.
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
    /// Whether this session exceeded the tenant's weekly AI session quota.
    /// Overage sessions are billed at the plan's overage rate.
    /// </summary>
    public bool IsOverage { get; set; }

    public virtual List<DispatchDecision> Decisions { get; } = [];

    public void Complete(string? summary = null)
    {
        Status = DispatchSessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Summary = summary;
    }

    public void Fail(string errorMessage)
    {
        Status = DispatchSessionStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }

    public void Cancel()
    {
        Status = DispatchSessionStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }
}
