using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

/// <summary>
/// Query to get loads for a customer in the portal.
/// </summary>
public class GetPortalLoadsQuery : SearchableQuery, IAppRequest<PagedResult<PortalLoadDto>>
{
    /// <summary>
    /// The customer ID to filter loads by (set from authenticated user context).
    /// </summary>
    public required Guid CustomerId { get; set; }

    /// <summary>
    /// Only return active (non-delivered) loads.
    /// </summary>
    public bool OnlyActiveLoads { get; set; }

    /// <summary>
    /// Start date filter.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date filter.
    /// </summary>
    public DateTime? EndDate { get; set; }
}
