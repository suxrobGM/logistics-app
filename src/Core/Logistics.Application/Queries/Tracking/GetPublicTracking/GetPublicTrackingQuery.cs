using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

/// <summary>
/// Query to get public tracking information for a load via tracking link.
/// </summary>
public class GetPublicTrackingQuery : IAppRequest<Result<PublicTrackingDto>>
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
