using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class UploadDocumentCommand : IAppRequest<Result<Guid>>
{
    public DocumentOwnerType OwnerType { get; set; }
    public Guid OwnerId { get; set; }

    public required Stream FileContent { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public DocumentType Type { get; set; }
    public string? Description { get; set; }
    public Guid UploadedById { get; set; }
}
