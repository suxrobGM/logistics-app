using Logistics.Models;

namespace Logistics.API.Controllers;

[Route("notifications")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseResult<AppRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.AppRole.View)]
    public async Task<IActionResult> GetList([FromQuery] GetAppRolesQuery query)
    {
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
}
