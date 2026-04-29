using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class LinkContainerToLoadCommand : IAppRequest<Result>
{
    public Guid ContainerId { get; set; }
    public Guid LoadId { get; set; }
}
