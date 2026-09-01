using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Documents.Queries;

public class GetDocumentByIdQuery : IQuery<Result<DocumentDto>>
{
    public Guid DocumentId { get; set; }
}
