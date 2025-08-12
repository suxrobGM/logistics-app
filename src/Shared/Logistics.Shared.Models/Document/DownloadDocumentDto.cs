namespace Logistics.Shared.Models;

public sealed class DocumentDownloadDto
{
    public required string FileName { get; init; }
    public required string OriginalFileName { get; init; }
    public required string ContentType { get; init; }
    public long FileSizeBytes { get; init; }
    public required Stream FileContent { get; init; }
}
