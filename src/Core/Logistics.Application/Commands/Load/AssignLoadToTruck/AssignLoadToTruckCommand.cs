using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class AssignLoadToTruckCommand : IAppRequest
{
    public Guid LoadId { get; set; }
    public Guid? TruckId { get; set; }
}
