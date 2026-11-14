using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Portal.Queries;

/// <summary>
/// Query to get loads for a customer in the portal.
/// </summary>
public class GetPortalLoadsQuery : SearchableQuery, IQuery<PagedResult<PortalLoadDto>>
{
    /// <summary>
    /// The customer ID to filter loads by (set from authenticated user context).
    /// </summary>
    public required Guid CustomerId { get; set; }

    public bool ActiveOnly { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}
