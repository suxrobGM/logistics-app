using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record DataExportRequestDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime RequestedAt { get; set; }
    public DataExportStatus Status { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Short-lived signed URL — only set when <see cref="Status"/> is Ready and the
    /// caller is authorized to download. Regenerated each time the request is fetched.
    /// </summary>
    public string? DownloadUrl { get; set; }
}
