using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class BulkDeleteLoadsCommand : IAppRequest
{
    public Guid[] LoadIds { get; set; } = [];
}
