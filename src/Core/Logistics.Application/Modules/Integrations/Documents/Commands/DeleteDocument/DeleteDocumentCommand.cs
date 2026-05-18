using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Integrations.Documents.Commands;

public class DeleteDocumentCommand : ICommand
{
    public Guid DocumentId { get; set; }
}
