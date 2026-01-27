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

    /// <summary>
    /// The user ID who reported the exception. Stored as reference only (no FK constraint)
    /// since users may exist in master DB.
    /// </summary>
    public Guid ReportedById { get; set; }

    /// <summary>
    /// Denormalized name of the user who reported the exception.
    /// </summary>
    public string ReportedByName { get; set; } = string.Empty;

    public string? Resolution { get; set; }

    public bool IsResolved => ResolvedAt.HasValue;
}
