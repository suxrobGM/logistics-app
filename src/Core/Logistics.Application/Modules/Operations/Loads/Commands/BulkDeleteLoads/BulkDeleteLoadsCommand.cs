using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

public class BulkDeleteLoadsCommand : ICommand
{
    public Guid[] LoadIds { get; set; } = [];
}
