using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class DownloadDocumentQuery : IAppRequest<Result<DocumentDownloadDto>>
{
    public Guid DocumentId { get; set; }
    public Guid RequestedById { get; set; }
}
