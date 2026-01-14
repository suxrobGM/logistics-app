using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("roles")]
[Produces("application/json")]
public class RoleController(IMediator mediator) : ControllerBase
{
    [HttpGet("app", Name = "GetAppRoles")]
    [ProducesResponseType(typeof(PagedResponse<RoleDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.AppRole.View)]
    public async Task<IActionResult> GetAppRoles([FromQuery] GetAppRolesQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<RoleDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpGet("tenant", Name = "GetTenantRoles")]
    [ProducesResponseType(typeof(PagedResponse<RoleDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.TenantRole.View)]
    public async Task<IActionResult> GetTenantRoles([FromQuery] GetTenantRolesQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<RoleDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpGet("{roleName}/permissions", Name = "GetRolePermissions")]
    [ProducesResponseType(typeof(PermissionDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPermissions(string roleName)
    {
        var result = await mediator.Send(new GetRolePermissionsQuery { RoleName = roleName });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }
}
