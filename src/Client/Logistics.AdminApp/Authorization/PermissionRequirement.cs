using Microsoft.AspNetCore.Authorization;

namespace Logistics.AdminApp.Authorization;

internal class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}
