using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class EmployeeDocument : Document
{
    public override DocumentOwnerType OwnerType { get; protected set; } = DocumentOwnerType.Employee;

    // Subject/owner: Employee
    public Guid EmployeeId { get; private set; }
    public virtual Employee Employee { get; private set; } = null!;

    public static EmployeeDocument Create(
        string fileName,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        string blobPath,
        string blobContainer,
        DocumentType type,
        Guid employeeId,
        Guid uploadedById,
        string? description = null)
    {
        var doc = new EmployeeDocument
        {
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            BlobPath = blobPath,
            BlobContainer = blobContainer,
            Type = type,
            Description = description,
            EmployeeId = employeeId,
            UploadedById = uploadedById,
            Status = DocumentStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        doc.DomainEvents.Add(new EmployeeDocumentUploadedEvent(doc.Id, employeeId));
        return doc;
    }

    protected override void OnStatusChanged(DocumentStatus newStatus)
    {
        if (newStatus == DocumentStatus.Deleted)
        {
            DomainEvents.Add(new EmployeeDocumentDeletedEvent(Id, EmployeeId));
        }
    }
}
