
namespace Logistics.Application.Modules.Operations.Trips.Commands;

public class MarkStopDepartedCommand : ICommand
{
    public Guid TripId { get; set; }
    public Guid StopId { get; set; }
}
