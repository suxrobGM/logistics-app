using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class AgentDecisionDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public AgentDecisionType Type { get; set; }
    public AgentDecisionStatus Status { get; set; }
    public string Reasoning { get; set; } = "";
    public string? ToolName { get; set; }
    public string? ToolInput { get; set; }

    /// <summary>
    /// The tool's result as a typed shape. Null when the tool ran but returned something outside
    /// <see cref="AgentToolResultDto"/>, which is most of them - the raw output stays on the entity
    /// for the model and the audit trail, and is deliberately not sent to the client.
    /// </summary>
    public AgentToolResultDto? ToolResult { get; set; }
    public Guid? LoadId { get; set; }
    public string? LoadName { get; set; }
    public Guid? TruckId { get; set; }
    public string? TruckNumber { get; set; }
    public Guid? TripId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public string? RejectionReason { get; set; }
}
