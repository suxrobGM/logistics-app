using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

public class LinkContainerToLoadCommand : ICommand<Result>
{
    public Guid ContainerId { get; set; }
    public Guid LoadId { get; set; }
}
