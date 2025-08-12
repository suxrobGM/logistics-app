using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public class DownloadDocumentQuery : IRequest<Result<DocumentDownloadDto>>
{
    public Guid DocumentId { get; set; }
    public Guid RequestedById { get; set; }
}
