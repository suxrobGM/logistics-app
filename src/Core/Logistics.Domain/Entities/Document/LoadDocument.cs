using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class LoadDocument : Document
{
    public override DocumentOwnerType OwnerType { get; protected set; } = DocumentOwnerType.Load;

    // Subject/owner: Load
    public Guid LoadId { get; private set; }
    public virtual Load Load { get; private set; } = null!;

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
        var doc = new LoadDocument
        {
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            BlobPath = blobPath,
            BlobContainer = blobContainer,
            Type = type,
            Description = description,
            LoadId = loadId,
            UploadedById = uploadedById,
            Status = DocumentStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        doc.DomainEvents.Add(new LoadDocumentUploadedEvent(doc.Id, loadId));
        return doc;
    }

    protected override void OnStatusChanged(DocumentStatus newStatus)
    {
        if (newStatus == DocumentStatus.Deleted)
            DomainEvents.Add(new LoadDocumentDeletedEvent(Id, LoadId));
    }
}
