using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents an individual decision made by the AI dispatch agent within a session.
/// Write-tool decisions start as Suggested and await dispatcher approval; read-tool decisions
/// execute immediately and are recorded for the audit trail.
/// </summary>
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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExecutedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public string? RejectionReason { get; set; }

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

    public void MarkFailed(string? errorOutput = null)
    {
        Status = AgentDecisionStatus.Failed;
        ToolOutput = errorOutput;
    }
}
