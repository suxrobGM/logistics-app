using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Documents.Queries;

public class DownloadDocumentQuery : IQuery<Result<DocumentDownloadDto>>
{
    public Guid DocumentId { get; set; }
    public Guid RequestedById { get; set; }
}
