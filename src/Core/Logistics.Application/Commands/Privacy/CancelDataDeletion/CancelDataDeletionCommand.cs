using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class CancelDataDeletionCommand : ICommand
{
    public Guid Id { get; set; }
}
