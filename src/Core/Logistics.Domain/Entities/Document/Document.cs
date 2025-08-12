using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public abstract class Document : AuditableEntity, ITenantEntity
{
    public abstract DocumentOwnerType OwnerType { get; protected set; }

    public required string FileName { get; set; }
    public required string OriginalFileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public required string BlobPath { get; set; }
    public required string BlobContainer { get; set; }
    public required DocumentType Type { get; set; }
    public DocumentStatus Status { get; protected set; } = DocumentStatus.Active;
    public string? Description { get; protected set; }

    public Guid UploadedById { get; set; }

    /// <summary>
    ///     Who uploaded this document.
    ///     This is used for audit purposes and to track who uploaded the document.
    /// </summary>
    public virtual Employee UploadedBy { get; set; } = null!;

    public void UpdateStatus(DocumentStatus status)
    {
        Status = status;
        LastModifiedAt = DateTime.UtcNow;
        OnStatusChanged(status);
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        LastModifiedAt = DateTime.UtcNow;
    }

    protected abstract void OnStatusChanged(DocumentStatus newStatus);
}
