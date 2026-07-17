using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Roles.Queries;

internal sealed class GetRolePermissionsHandler(IMasterUnitOfWork masterUow, ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetRolePermissionsQuery, Result<PermissionDto[]>>
{
    public Task<Result<PermissionDto[]>> Handle(GetRolePermissionsQuery req, CancellationToken ct)
    {
        if (req.RoleName.StartsWith("app"))
        {
            return GetAppRolePermissions(req);
        }

        if (req.RoleName.StartsWith("tenant"))
        {
            return GetTenantRolePermissions(req);
        }

        return Task.FromResult(Result<PermissionDto[]>.Fail($"Invalid role name '{req.RoleName}'"));
    }

    private async Task<Result<PermissionDto[]>> GetAppRolePermissions(GetRolePermissionsQuery req)
    {
        var role = await masterUow.Repository<AppRole>().GetAsync(i => i.Name == req.RoleName);

        if (role is null)
        {
            return Result<PermissionDto[]>.Fail($"Could not find a role with name '{req.RoleName}'");
        }

        var permissions = await masterUow.Repository<AppRoleClaim, int>().GetListAsync(i => i.RoleId == role.Id);
        var permissionsDto = permissions.Select(i => i.ToDto()).ToArray();
        return Result<PermissionDto[]>.Ok(permissionsDto);
    }

    private async Task<Result<PermissionDto[]>> GetTenantRolePermissions(GetRolePermissionsQuery req)
    {
        var role = await tenantUow.Repository<TenantRole>().GetAsync(i => i.Name == req.RoleName);

        if (role is null)
        {
            return Result<PermissionDto[]>.Fail($"Could not find a role with name '{req.RoleName}'");
        }

        var permissions = await tenantUow.Repository<TenantRoleClaim>().GetListAsync(i => i.RoleId == role.Id);
        var permissionsDto = permissions.Select(i => i.ToDto()).ToArray();
        return Result<PermissionDto[]>.Ok(permissionsDto);
    }
}
