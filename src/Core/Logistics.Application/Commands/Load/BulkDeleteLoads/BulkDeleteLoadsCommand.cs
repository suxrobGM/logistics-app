using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class BulkDeleteLoadsCommand : ICommand
{
    public Guid[] LoadIds { get; set; } = [];
}
