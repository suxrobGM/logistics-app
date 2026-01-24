using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class TruckDocument : Document
{
    public override DocumentOwnerType OwnerType { get; protected set; } = DocumentOwnerType.Truck;

    // Subject/owner: Truck
    public Guid TruckId { get; private set; }
    public virtual Truck Truck { get; private set; } = null!;

    public static TruckDocument Create(
        string fileName,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        string blobPath,
        string blobContainer,
        DocumentType type,
        Guid truckId,
        Guid uploadedById,
        string? description = null)
    {
        var doc = new TruckDocument
        {
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            BlobPath = blobPath,
            BlobContainer = blobContainer,
            Type = type,
            Description = description,
            TruckId = truckId,
            UploadedById = uploadedById,
            Status = DocumentStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        doc.DomainEvents.Add(new TruckDocumentUploadedEvent(doc.Id, truckId));
        return doc;
    }

    protected override void OnStatusChanged(DocumentStatus newStatus)
    {
        if (newStatus == DocumentStatus.Deleted)
        {
            DomainEvents.Add(new TruckDocumentDeletedEvent(Id, TruckId));
        }
    }
}
