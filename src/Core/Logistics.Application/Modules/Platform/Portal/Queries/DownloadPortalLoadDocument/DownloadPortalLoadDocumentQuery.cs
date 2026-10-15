using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

/// <summary>
/// Downloads a document of one of the customer's loads in the customer portal.
/// </summary>
public class DownloadPortalLoadDocumentQuery : IQuery<Result<DocumentDownloadDto>>
{
    public required Guid LoadId { get; set; }

    public required Guid DocumentId { get; set; }

    /// <summary>
    /// The customer ID (for authorization - set from authenticated user context).
    /// </summary>
    public required Guid CustomerId { get; set; }
}
