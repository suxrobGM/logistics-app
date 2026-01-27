using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class BulkDispatchLoadsCommand : IAppRequest
{
    public Guid[] LoadIds { get; set; } = [];
}
