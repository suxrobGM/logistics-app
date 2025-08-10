using Logistics.Domain.Core;
using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class LoadDocument : Entity, ITenantEntity
{
    public required string FileName { get; set; }
    public required string OriginalFileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public required string BlobPath { get; set; }
    public required string BlobContainer { get; set; }
    public required DocumentType Type { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Active;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Guid LoadId { get; set; }
    public virtual Load Load { get; set; } = null!;
    
    public Guid UploadedById { get; set; }
    public virtual Employee UploadedBy { get; set; } = null!;

    public static LoadDocument Create(
        string fileName,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        string blobPath,
        string blobContainer,
        DocumentType type,
        Guid loadId,
        Guid uploadedById,
        string? description = null)
    {
        var document = new LoadDocument
        {
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            BlobPath = blobPath,
            BlobContainer = blobContainer,
            Type = type,
            LoadId = loadId,
            UploadedById = uploadedById,
            Description = description,
            Status = DocumentStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        document.DomainEvents.Add(new LoadDocumentUploadedEvent(document.Id, loadId));
        return document;
    }

    public void UpdateStatus(DocumentStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
        
        if (status == DocumentStatus.Deleted)
        {
            DomainEvents.Add(new LoadDocumentDeletedEvent(Id, LoadId));
        }
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}
