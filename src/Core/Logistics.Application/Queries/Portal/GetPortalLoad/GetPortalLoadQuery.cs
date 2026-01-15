using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

/// <summary>
/// Query to get a specific load for a customer in the portal.
/// </summary>
public class GetPortalLoadQuery : IAppRequest<Result<PortalLoadDto>>
{
    /// <summary>
    /// The load ID to retrieve.
    /// </summary>
    public required Guid LoadId { get; set; }

    /// <summary>
    /// The customer ID (for authorization - set from authenticated user context).
    /// </summary>
    public required Guid CustomerId { get; set; }
}
