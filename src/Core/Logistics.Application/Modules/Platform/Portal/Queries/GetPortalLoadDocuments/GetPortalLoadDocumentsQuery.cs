using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

/// <summary>
/// Query to get documents for a load in the customer portal.
/// </summary>
public class GetPortalLoadDocumentsQuery : IQuery<Result<IEnumerable<DocumentDto>>>
{
    /// <summary>
    /// The load ID to get documents for.
    /// </summary>
    public required Guid LoadId { get; set; }

    /// <summary>
    /// The customer ID (for authorization - set from authenticated user context).
    /// </summary>
    public required Guid CustomerId { get; set; }
}
