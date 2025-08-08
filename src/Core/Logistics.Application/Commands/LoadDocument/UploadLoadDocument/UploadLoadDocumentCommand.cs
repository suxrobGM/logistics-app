using Logistics.Shared.Consts;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class UploadLoadDocumentCommand : IRequest<Result<Guid>>
{
    public Guid LoadId { get; set; }
    public required Stream FileContent { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public DocumentType Type { get; set; }
    public string? Description { get; set; }
    public Guid UploadedById { get; set; }
}