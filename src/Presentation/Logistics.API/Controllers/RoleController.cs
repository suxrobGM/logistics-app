using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("roles")]
public class RoleController(IMediator mediator) : ControllerBase
{
    [HttpGet("app", Name = "GetAppRoles")]
    [ProducesResponseType(typeof(PagedResult<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.AppRoles.View)]
    public async Task<IActionResult> GetAppRoles([FromQuery] GetAppRolesQuery query)
    {
        var result = await mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("tenant", Name = "GetTenantRoles")]
    [ProducesResponseType(typeof(PagedResult<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.TenantRoles.View)]
    public async Task<IActionResult> GetTenantRoles([FromQuery] GetTenantRolesQuery query)
    {
        var result = await mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{roleName}/permissions", Name = "GetRolePermissions")]
    [ProducesResponseType(typeof(Result<PermissionDto[]>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPermissions(string roleName)
    {
        var result = await mediator.Send(new GetRolePermissionsQuery { RoleName = roleName });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
