using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Documents.Queries;

public class GetDocumentByIdQuery : IQuery<Result<DocumentDto>>
{
    public Guid DocumentId { get; set; }
}
