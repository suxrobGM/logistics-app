using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
///     A document captured during delivery (POD/BOL) with associated metadata
///     such as recipient signature, GPS coordinates, and timestamp.
/// </summary>
public class DeliveryDocument : LoadDocument
{
    public override DocumentOwnerType OwnerType { get; protected set; } = DocumentOwnerType.Delivery;

    // POD/BOL capture metadata
    public string? RecipientName { get; set; }
    public string? RecipientSignature { get; set; }  // Base64 encoded signature or blob path
    public double? CaptureLatitude { get; set; }
    public double? CaptureLongitude { get; set; }
    public DateTime? CapturedAt { get; set; }
    public Guid? TripStopId { get; set; }  // The trip stop this POD/BOL is associated with
    public string? Notes { get; set; }  // Additional notes from driver

    public virtual TripStop? TripStop { get; private set; }

    public static DeliveryDocument Create(
        string fileName,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        string blobPath,
        string blobContainer,
        DocumentType type,
        Guid loadId,
        Guid uploadedById,
        string? recipientName = null,
        string? recipientSignature = null,
        double? captureLatitude = null,
        double? captureLongitude = null,
        DateTime? capturedAt = null,
        Guid? tripStopId = null,
        string? notes = null)
    {
        var doc = new DeliveryDocument
        {
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            BlobPath = blobPath,
            BlobContainer = blobContainer,
            Type = type,
            Description = notes,
            LoadId = loadId,
            UploadedById = uploadedById,
            Status = DocumentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            RecipientName = recipientName,
            RecipientSignature = recipientSignature,
            CaptureLatitude = captureLatitude,
            CaptureLongitude = captureLongitude,
            CapturedAt = capturedAt ?? DateTime.UtcNow,
            TripStopId = tripStopId,
            Notes = notes
        };

        doc.DomainEvents.Add(new DeliveryDocumentCreatedEvent(doc.Id, loadId, type));
        return doc;
    }

    protected override void OnStatusChanged(DocumentStatus newStatus)
    {
        if (newStatus == DocumentStatus.Deleted)
        {
            DomainEvents.Add(new LoadDocumentDeletedEvent(Id, LoadId));
        }
    }
}
