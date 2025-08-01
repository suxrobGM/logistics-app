using Logistics.Application.Queries;
using Logistics.Shared.Models;
using Logistics.Domain.Primitives.Enums.Policies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[Route("roles")]
[ApiController]
public class RoleController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("app")]
    [ProducesResponseType(typeof(PagedResult<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.AppRoles.View)]
    public async Task<IActionResult> GetAppRoles([FromQuery] GetAppRolesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpGet("tenant")]
    [ProducesResponseType(typeof(PagedResult<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.TenantRoles.View)]
    public async Task<IActionResult> GetTenantRoles([FromQuery] GetTenantRolesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpGet("{roleName}/permissions")]
    [ProducesResponseType(typeof(Result<PermissionDto[]>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPermissions(string roleName)
    {
        var result = await _mediator.Send(new GetRolePermissionsQuery {RoleName = roleName});
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
