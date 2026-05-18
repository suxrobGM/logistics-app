using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

/// <summary>
/// Query to get all tenant access records for a portal user.
/// Returns the list of tenants/companies the user can access.
/// </summary>
public class GetUserTenantAccessQuery : IQuery<Result<List<UserTenantAccessDto>>>
{
    /// <summary>
    /// The user ID (from authentication token).
    /// </summary>
    public required Guid UserId { get; set; }
}
