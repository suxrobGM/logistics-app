using Logistics.API.Extensions;
using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared;
using Logistics.Shared.Models;
using Logistics.Shared.Consts.Policies;
using Logistics.Shared.Consts.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[Route("tenants")]
[ApiController]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{identifier}")]
    [ProducesResponseType(typeof(Result<TenantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> GetById(string identifier)
    {
        var includeConnectionString = HttpContext.User.HasOneTheseRoles(AppRoles.SuperAdmin, AppRoles.Admin);
        var result = await _mediator.Send(new GetTenantQuery
        {
            Id = identifier,
            Name = identifier,
            IncludeConnectionString = includeConnectionString
        });

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LoadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Tenants.View)]
    public async Task<IActionResult> GetList([FromQuery] GetTenantsQuery query)
    {
        if (User.HasOneTheseRoles(AppRoles.SuperAdmin, AppRoles.Admin))
        {
            query.IncludeConnectionStrings = true;
        }

        var result = await _mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Tenants.Create)]
    public async Task<IActionResult> Create([FromBody] CreateTenantCommand request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Tenants.Edit)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateTenantCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Tenants.Delete)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteTenantCommand {Id = id});
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
