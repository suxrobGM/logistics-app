using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class BulkAssignLoadsCommand : IAppRequest
{
    public Guid[] LoadIds { get; set; } = [];
    public Guid TruckId { get; set; }
}
