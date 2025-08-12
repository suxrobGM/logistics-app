using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetRolePermissionsQuery : IAppRequest<Result<PermissionDto[]>>
{
    public string RoleName { get; set; } = null!;
}
