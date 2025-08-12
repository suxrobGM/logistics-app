namespace Logistics.Shared.Models;

public class LoadDocumentDownloadDto
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public Stream FileContent { get; set; } = Stream.Null;
}
