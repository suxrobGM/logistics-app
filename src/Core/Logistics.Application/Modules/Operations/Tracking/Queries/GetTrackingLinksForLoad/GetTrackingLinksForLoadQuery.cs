using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Tracking.Queries;

/// <summary>
/// Query to get all tracking links for a specific load.
/// </summary>
public class GetTrackingLinksForLoadQuery : IQuery<Result<IEnumerable<TrackingLinkDto>>>
{
    /// <summary>
    /// The load ID to get tracking links for.
    /// </summary>
    public required Guid LoadId { get; set; }
}
