using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

public class DispatchLoadCommand : ICommand
{
    public Guid Id { get; set; }
}
