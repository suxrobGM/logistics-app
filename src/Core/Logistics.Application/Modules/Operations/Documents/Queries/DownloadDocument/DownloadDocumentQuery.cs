using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Documents.Queries;

public class DownloadDocumentQuery : IQuery<Result<DocumentDownloadDto>>
{
    public Guid DocumentId { get; set; }
}
