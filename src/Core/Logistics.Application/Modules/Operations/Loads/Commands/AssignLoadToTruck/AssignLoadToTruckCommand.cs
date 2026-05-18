using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

public class AssignLoadToTruckCommand : ICommand
{
    public Guid LoadId { get; set; }
    public Guid? TruckId { get; set; }
}
