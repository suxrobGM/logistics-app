using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public sealed record DocumentDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = null!;
    public string OriginalFileName { get; init; } = null!;
    public string ContentType { get; init; } = null!;
    public long FileSizeBytes { get; init; }
    public string BlobPath { get; init; } = null!;
    public string BlobContainer { get; init; } = null!;
    public DocumentType Type { get; init; }
    public DocumentStatus Status { get; init; }
    public string? Description { get; init; }

    public Guid UploadedById { get; init; }

    // Owner (exactly one will be set)
    public Guid? LoadId { get; init; }
    public Guid? EmployeeId { get; init; }

    // POD/BOL capture metadata
    public string? RecipientName { get; init; }
    public string? RecipientSignature { get; init; }
    public double? CaptureLatitude { get; init; }
    public double? CaptureLongitude { get; init; }
    public DateTime? CapturedAt { get; init; }
    public Guid? TripStopId { get; init; }
    public string? Notes { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
