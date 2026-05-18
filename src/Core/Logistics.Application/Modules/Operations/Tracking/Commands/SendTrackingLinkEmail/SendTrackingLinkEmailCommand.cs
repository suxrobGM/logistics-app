using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Tracking.Commands;

public class SendTrackingLinkEmailCommand : ICommand<Result>
{
    public required Guid TrackingLinkId { get; set; }
    public required string RecipientEmail { get; set; }
    public string? PersonalMessage { get; set; }
}
