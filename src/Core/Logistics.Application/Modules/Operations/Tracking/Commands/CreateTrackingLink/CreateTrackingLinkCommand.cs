using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Tracking.Commands;

public class CreateTrackingLinkCommand : ICommand<Result<TrackingLinkDto>>
{
    public required Guid LoadId { get; set; }
}
