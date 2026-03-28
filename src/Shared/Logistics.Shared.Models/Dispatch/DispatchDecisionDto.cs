using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class DispatchDecisionDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public DispatchDecisionType Type { get; set; }
    public DispatchDecisionStatus Status { get; set; }
    public string Reasoning { get; set; } = "";
    public string? ToolName { get; set; }
    public string? ToolInput { get; set; }
    public string? ToolOutput { get; set; }
    public Guid? LoadId { get; set; }
    public string? LoadName { get; set; }
    public Guid? TruckId { get; set; }
    public string? TruckNumber { get; set; }
    public Guid? TripId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public string? RejectionReason { get; set; }
}
