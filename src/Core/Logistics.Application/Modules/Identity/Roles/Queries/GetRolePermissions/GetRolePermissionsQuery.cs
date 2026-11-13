using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.Roles.Queries;

public class GetRolePermissionsQuery : IQuery<Result<PermissionDto[]>>
{
    public string RoleName { get; set; } = null!;
}
