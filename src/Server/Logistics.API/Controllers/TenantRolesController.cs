using Logistics.Application.Tenant.Queries;
using Logistics.Shared;
using Logistics.Shared.Models;
using Logistics.Shared.Consts.Policies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[Route("tenant-roles")]
[ApiController]
public class TenantRolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantRolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TenantRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.TenantRoles.View)]
    public async Task<IActionResult> GetList([FromQuery] GetTenantRolesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
