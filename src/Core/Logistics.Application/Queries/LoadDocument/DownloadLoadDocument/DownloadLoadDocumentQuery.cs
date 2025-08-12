using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public class DownloadLoadDocumentQuery : IRequest<Result<LoadDocumentDownloadDto>>
{
    public Guid DocumentId { get; set; }
    public Guid RequestedById { get; set; }
}
