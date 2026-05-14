using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class AssignLoadToTruckCommand : ICommand
{
    public Guid LoadId { get; set; }
    public Guid? TruckId { get; set; }
}
