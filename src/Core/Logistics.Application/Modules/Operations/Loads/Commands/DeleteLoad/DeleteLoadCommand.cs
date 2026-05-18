using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

public class DeleteLoadCommand : ICommand
{
    public Guid Id { get; set; }
}
