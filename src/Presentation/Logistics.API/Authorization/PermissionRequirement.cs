using Microsoft.AspNetCore.Authorization;

namespace Logistics.API.Authorization;

internal class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
