using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class LoadDocumentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DocumentType Type { get; set; }
    public DocumentStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Load information
    public Guid LoadId { get; set; }
    public string? LoadName { get; set; }
    public long? LoadNumber { get; set; }
    
    // Uploader information
    public Guid UploadedById { get; set; }
    public string? UploadedByName { get; set; }
    public string? UploadedByEmail { get; set; }
}
