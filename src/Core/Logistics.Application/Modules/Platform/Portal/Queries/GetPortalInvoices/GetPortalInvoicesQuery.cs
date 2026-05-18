using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

/// <summary>
/// Query to get invoices for a customer in the portal.
/// </summary>
public class GetPortalInvoicesQuery : SearchableQuery, IQuery<PagedResult<PortalInvoiceDto>>
{
    /// <summary>
    /// The customer ID to filter invoices by (set from authenticated user context).
    /// </summary>
    public required Guid CustomerId { get; set; }

    /// <summary>
    /// Start date filter.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date filter.
    /// </summary>
    public DateTime? EndDate { get; set; }
}
