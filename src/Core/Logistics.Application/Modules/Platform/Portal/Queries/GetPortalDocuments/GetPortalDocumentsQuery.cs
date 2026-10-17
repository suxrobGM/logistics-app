using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

/// <summary>
/// Query to get the documents of every load of a customer in the customer portal, newest first.
/// </summary>
public class GetPortalDocumentsQuery : IQuery<Result<IEnumerable<DocumentDto>>>
{
    /// <summary>
    /// The customer ID (for authorization - set from authenticated user context).
    /// </summary>
    public required Guid CustomerId { get; set; }
}
