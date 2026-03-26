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

    public DispatchAgentMode Mode { get; set; }
    public DispatchSessionStatus Status { get; private set; } = DispatchSessionStatus.Running;

    /// <summary>
    /// The user who triggered this session. Null if triggered by background job.
    /// </summary>
    public Guid? TriggeredByUserId { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Total Claude API tokens consumed during this session.
    /// </summary>
    public int TotalTokensUsed { get; set; }

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
