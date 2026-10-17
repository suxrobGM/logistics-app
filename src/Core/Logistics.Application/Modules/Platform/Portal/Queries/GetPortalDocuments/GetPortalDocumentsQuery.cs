using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

/// <summary>
/// The documents of every load a customer has, newest first.
/// </summary>
public class GetPortalDocumentsQuery : IQuery<Result<PortalDocumentDto[]>>
{
    /// <summary>
    /// The customer ID (for authorization - set from authenticated user context).
    /// </summary>
    public required Guid CustomerId { get; set; }
}
