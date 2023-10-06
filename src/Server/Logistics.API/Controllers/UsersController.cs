using Logistics.Models;

namespace Logistics.API.Controllers;

[Route("users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.User.View)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery {UserId = id});

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result.Error);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.User.View)]
    public async Task<IActionResult> GetList([FromQuery] GetUsersQuery query)
    {
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpGet("{id}/organizations")]
    [ProducesResponseType(typeof(ResponseResult<OrganizationDto[]>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.User.View)]
    public async Task<IActionResult> GetUserJoinedOrganizations(string id)
    {
        var result = await _mediator.Send(new GetUserJoinedOrganizationsQuery() { UserId = id });

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.User.View)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("{id}/remove-role")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.User.Edit)]
    public async Task<IActionResult> RemoveRole(string id, [FromBody] RemoveRoleFromUserCommand request)
    {
        request.UserId = id;
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
}
