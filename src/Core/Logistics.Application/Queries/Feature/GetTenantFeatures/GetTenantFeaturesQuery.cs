using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

/// <summary>
/// Query to get all feature statuses for a tenant.
/// </summary>
public class GetTenantFeaturesQuery : IAppRequest<Result<IReadOnlyList<FeatureStatusDto>>>
{
    /// <summary>
    /// The tenant ID. If null, uses the current tenant from context.
    /// </summary>
    public Guid? TenantId { get; set; }
}
