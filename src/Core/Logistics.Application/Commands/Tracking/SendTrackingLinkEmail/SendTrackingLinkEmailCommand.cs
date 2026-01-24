using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class SendTrackingLinkEmailCommand : IAppRequest<Result>
{
    public required Guid TrackingLinkId { get; set; }
    public required string RecipientEmail { get; set; }
    public string? PersonalMessage { get; set; }
}
