using Logistics.Application.Abstractions.Common;

namespace Logistics.Application.Modules.IdentityAccess.Users.Services;

/// <summary>
/// The caller's effective permissions, cached per user and tenant. Resolving them costs ~9 queries
/// across both databases, so consumers come through here, never through the query directly.
/// </summary>
public interface IUserPermissionService : IApplicationService
{
    Task<IReadOnlySet<string>> GetPermissionsAsync(
        Guid userId, Guid? tenantId, CancellationToken ct = default);

    Task<bool> HasPermissionAsync(
        Guid userId, Guid? tenantId, string permission, CancellationToken ct = default);
}
