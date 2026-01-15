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

    // POD/BOL capture metadata (optional - only populated for proof of delivery/bill of lading documents)
    public string? RecipientName { get; set; }
    public string? RecipientSignature { get; set; }  // Base64 encoded signature or blob path
    public double? CaptureLatitude { get; set; }
    public double? CaptureLongitude { get; set; }
    public DateTime? CapturedAt { get; set; }
    public Guid? TripStopId { get; set; }  // The trip stop this POD/BOL is associated with
    public string? Notes { get; set; }  // Additional notes from driver

    public Guid UploadedById { get; set; }

    /// <summary>
    ///     Who uploaded this document.
    ///     This is used for audit purposes and to track who uploaded the document.
    /// </summary>
    public virtual Employee UploadedBy { get; set; } = null!;

    public void UpdateStatus(DocumentStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
        OnStatusChanged(status);
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    protected abstract void OnStatusChanged(DocumentStatus newStatus);
}
