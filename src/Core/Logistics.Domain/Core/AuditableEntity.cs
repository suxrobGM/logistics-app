namespace Logistics.Domain.Core;

/// <summary>
/// Adds immutable Created* fields and mutable LastModified* fields
/// to every domain entity.
/// </summary>
public abstract class AuditableEntity : Entity, IAuditableEntity
{
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public void SetCreated(string? userId)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        CreatedBy = userId;
    }

    public void SetModified(string? userId)
    {
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = userId;
    }
}
