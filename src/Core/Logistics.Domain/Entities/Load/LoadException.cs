using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents an exception or issue encountered during load transportation.
/// </summary>
public class LoadException : AuditableEntity, ITenantEntity
{
    public Guid LoadId { get; set; }
    public virtual Load Load { get; set; } = null!;

    public LoadExceptionType Type { get; set; }
    public required string Reason { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public Guid ReportedById { get; set; }
    public virtual User ReportedBy { get; set; } = null!;

    public string? Resolution { get; set; }

    public bool IsResolved => ResolvedAt.HasValue;
}
