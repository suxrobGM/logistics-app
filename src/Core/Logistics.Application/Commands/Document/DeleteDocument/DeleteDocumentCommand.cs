using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteDocumentCommand : ICommand
{
    public Guid DocumentId { get; set; }
}
