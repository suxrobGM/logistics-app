using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents an individual decision made by the AI dispatch agent within a session.
/// In HumanInTheLoop mode, decisions start as Suggested and await approval.
/// In Autonomous mode, decisions are executed immediately.
/// </summary>
public class DispatchDecision : Entity, ITenantEntity
{
    public Guid SessionId { get; set; }
    public virtual DispatchSession Session { get; set; } = null!;

    public DispatchDecisionType Type { get; set; }
    public DispatchDecisionStatus Status { get; set; } = DispatchDecisionStatus.Suggested;

    /// <summary>
    /// The agent's explanation for this decision.
    /// </summary>
    public string Reasoning { get; set; } = "";

    /// <summary>
    /// The Claude tool that was called.
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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExecutedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public string? RejectionReason { get; set; }

    public void Approve(Guid userId)
    {
        Status = DispatchDecisionStatus.Approved;
        ApprovedByUserId = userId;
    }

    public void Reject(Guid userId, string? reason = null)
    {
        Status = DispatchDecisionStatus.Rejected;
        ApprovedByUserId = userId;
        RejectionReason = reason;
    }

    public void MarkExecuted()
    {
        Status = DispatchDecisionStatus.Executed;
        ExecutedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string? errorOutput = null)
    {
        Status = DispatchDecisionStatus.Failed;
        ToolOutput = errorOutput;
    }
}
