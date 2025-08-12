using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public class GetRolePermissionsQuery : IRequest<Result<PermissionDto[]>>
{
    public string RoleName { get; set; } = null!;
}
