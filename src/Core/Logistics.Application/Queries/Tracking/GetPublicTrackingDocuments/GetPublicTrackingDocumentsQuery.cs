using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

/// <summary>
/// Query to get documents for a load via public tracking link.
/// </summary>
public class GetPublicTrackingDocumentsQuery : IAppRequest<Result<IEnumerable<DocumentDto>>>
{
    /// <summary>
    /// The tenant ID that owns the tracking link.
    /// </summary>
    public required Guid TenantId { get; set; }

    /// <summary>
    /// The tracking link token.
    /// </summary>
    public required string Token { get; set; }
}
