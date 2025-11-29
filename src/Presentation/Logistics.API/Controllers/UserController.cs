using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("users")]
public class UserController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id}", Name = "GetUserById")]
    [ProducesResponseType(typeof(Result<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Users.View)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await mediator.Send(new GetUserByIdQuery { UserId = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet(Name = "GetUsers")]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Users.View)]
    public async Task<IActionResult> GetList([FromQuery] GetUsersQuery query)
    {
        var result = await mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}/tenant", Name = "GetUserCurrentTenant")]
    [ProducesResponseType(typeof(Result<TenantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Users.View)]
    public async Task<IActionResult> GetUserCurrentTenant(Guid id)
    {
        var result = await mediator.Send(new GetUserCurrentTenantQuery { UserId = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}", Name = "UpdateUser")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Users.View)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/remove-role", Name = "RemoveRoleFromUser")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Users.Edit)]
    public async Task<IActionResult> RemoveRole(string id, [FromBody] RemoveRoleFromUserCommand request)
    {
        request.UserId = id;
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
