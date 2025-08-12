using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteDocumentCommand : IAppRequest
{
    public Guid DocumentId { get; set; }
}
