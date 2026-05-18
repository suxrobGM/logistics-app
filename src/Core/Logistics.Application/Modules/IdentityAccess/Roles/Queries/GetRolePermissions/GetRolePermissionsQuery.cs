using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Roles.Queries;

public class GetRolePermissionsQuery : IQuery<Result<PermissionDto[]>>
{
    public string RoleName { get; set; } = null!;
}
