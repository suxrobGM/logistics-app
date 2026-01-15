namespace Logistics.Shared.Models;

/// <summary>
/// Represents a file upload with stream content, typically used for photos, documents, etc.
/// </summary>
public class FileUpload
{
    public required Stream Content { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
}
