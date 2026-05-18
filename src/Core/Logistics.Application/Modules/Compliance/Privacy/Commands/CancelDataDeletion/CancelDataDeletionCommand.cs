using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Compliance.Privacy.Commands;

public class CancelDataDeletionCommand : ICommand
{
    public Guid Id { get; set; }
}
