using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>A tool call the agent made in a session. Write-tool calls wait for human approval; read-tool calls run immediately and are recorded.</summary>
public class AgentDecision : Entity, ITenantEntity
{
    public Guid SessionId { get; set; }
    public virtual AgentSession Session { get; set; } = null!;

    public AgentDecisionType Type { get; set; }
    public AgentDecisionStatus Status { get; set; } = AgentDecisionStatus.Suggested;

    /// <summary>
    /// The agent's explanation for this decision.
    /// </summary>
    public string Reasoning { get; set; } = "";

    /// <summary>
    /// The agent tool that was called.
    /// </summary>
    public string? ToolName { get; set; }

    /// <summary>
    /// JSON-serialized tool input parameters.
    /// </summary>
    public string? ToolInput { get; set; }

    /// <summary>
    /// JSON-serialized tool output result.
    /// </summary>
    public string? ToolOutput { get; set; }

    public Guid? LoadId { get; set; }
    public Guid? TruckId { get; set; }
    public Guid? TripId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? NegotiationId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExecutedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public string? RejectionReason { get; set; }

    public static readonly TimeSpan RevertWindow = TimeSpan.FromMinutes(15);

    /// <summary><see cref="ApprovedByUserId"/> stays null, which keeps it out of policy learning.</summary>
    public bool AutoApproved { get; set; }

    /// <summary>Counts as a rejection for the trust streak.</summary>
    public DateTime? RevertedAt { get; private set; }

    public bool CanRevert(DateTime now) =>
        AutoApproved
        && Status == AgentDecisionStatus.Executed
        && RevertedAt is null
        && ExecutedAt is { } executedAt
        && now - executedAt <= RevertWindow;

    public void Approve(Guid userId)
    {
        Status = AgentDecisionStatus.Approved;
        ApprovedByUserId = userId;
    }

    public void Reject(Guid userId, string? reason = null)
    {
        Status = AgentDecisionStatus.Rejected;
        ApprovedByUserId = userId;
        RejectionReason = reason;
    }

    public void MarkExecuted()
    {
        Status = AgentDecisionStatus.Executed;
        ExecutedAt = DateTime.UtcNow;
    }

    public void MarkAutoExecuted()
    {
        AutoApproved = true;
        MarkExecuted();
    }

    public void MarkReverted(DateTime now)
    {
        RevertedAt = now;
    }

    public void MarkFailed(string? errorOutput = null)
    {
        Status = AgentDecisionStatus.Failed;
        ToolOutput = errorOutput;
    }
}
