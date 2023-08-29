using Logistics.Models;

namespace Logistics.API.Controllers;

[Route("[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.User.View)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery
        {
            UserId = id
        });

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(PagedResponseResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.User.View)]
    public async Task<IActionResult> GetList([FromQuery] GetUsersQuery query)
    {
        var result = await _mediator.Send(query);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpGet("{id}/organizations")]
    [ProducesResponseType(typeof(PagedResponseResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.User.View)]
    public async Task<IActionResult> GetList(string id)
    {
        var result = await _mediator.Send(new GetUserJoinedOrganizationsQuery() { UserId = id });

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPut("update/{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.User.View)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("removeRole")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.User.Edit)]
    public async Task<IActionResult> RemoveRole([FromBody] RemoveUserRoleCommand request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}
