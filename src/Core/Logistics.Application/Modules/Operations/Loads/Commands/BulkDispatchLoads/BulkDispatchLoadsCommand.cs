using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

public class BulkDispatchLoadsCommand : ICommand
{
    public Guid[] LoadIds { get; set; } = [];
}
