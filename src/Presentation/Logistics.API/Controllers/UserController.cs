using Logistics.API.Extensions;
using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Claims;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("users")]
[Produces("application/json")]
public class UserController(IMediator mediator) : ControllerBase
{
    [HttpGet("me/permissions", Name = "GetCurrentUserPermissions")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> GetCurrentUserPermissions()
    {
        var userId = User.GetUserId();
        var tenantClaim = User.FindFirst(CustomClaimTypes.Tenant)?.Value;
        var tenantId = Guid.TryParse(tenantClaim, out var tid) ? tid : (Guid?)null;

        var result = await mediator.Send(new GetCurrentUserPermissionsQuery
        {
            UserId = userId,
            TenantId = tenantId
        });

        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("{id}", Name = "GetUserById")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.User.View)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await mediator.Send(new GetUserByIdQuery { UserId = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetUsers")]
    [ProducesResponseType(typeof(PagedResponse<UserDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.User.View)]
    public async Task<IActionResult> GetList([FromQuery] GetUsersQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<UserDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpGet("{id:guid}/tenant", Name = "GetUserCurrentTenant")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.User.View)]
    public async Task<IActionResult> GetUserCurrentTenant(Guid id)
    {
        var result = await mediator.Send(new GetUserCurrentTenantQuery { UserId = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.User.View)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("{id}/remove-role", Name = "RemoveRoleFromUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.User.Manage)]
    public async Task<IActionResult> RemoveRole(string id, [FromBody] RemoveRoleFromUserCommand request)
    {
        request.UserId = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
