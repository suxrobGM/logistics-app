using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

public class BulkAssignLoadsCommand : ICommand
{
    public Guid[] LoadIds { get; set; } = [];
    public Guid TruckId { get; set; }
}
