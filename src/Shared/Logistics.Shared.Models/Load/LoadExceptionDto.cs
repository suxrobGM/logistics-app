using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class LoadExceptionDto
{
    public Guid Id { get; set; }
    public LoadExceptionType Type { get; set; }
    public required string Reason { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public required string ReportedByName { get; set; }
    public string? Resolution { get; set; }
}
