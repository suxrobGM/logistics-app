using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Tracking.Commands;

/// <summary>
/// Records a public-tracking-link access (counter + timestamp). Enqueued fire-and-forget
/// from the <c>GetPublicTracking</c> query handler so the read path stays write-free.
/// </summary>
public class RecordTrackingAccessCommand : ICommand
{
    public Guid TenantId { get; set; }
    public Guid TrackingLinkId { get; set; }
}
