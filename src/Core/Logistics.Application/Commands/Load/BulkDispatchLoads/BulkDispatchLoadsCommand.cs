using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class BulkDispatchLoadsCommand : ICommand
{
    public Guid[] LoadIds { get; set; } = [];
}
