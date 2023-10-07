namespace Logistics.Domain.Abstractions;

public abstract class AuditableEntity : Entity, IAuditableEntity
{
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}
