
namespace Logistics.Application.Modules.Operations.Documents.Commands;

public class DeleteDocumentCommand : ICommand
{
    public Guid DocumentId { get; set; }
}
