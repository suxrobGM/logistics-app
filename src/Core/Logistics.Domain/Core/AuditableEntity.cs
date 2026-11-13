namespace Logistics.Domain.Core;

/// <summary>
///     Adds immutable Created* fields and mutable Updated* fields
///     to every domain entity.
/// </summary>
public abstract class AuditableEntity : Entity, IAuditableEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
