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
    [ProducesResponseType(typeof(ResponseResult<NotificationDto[]>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.AppRole.View)]
    public async Task<IActionResult> GetList([FromQuery] GetNotificationsQuery request)
    {
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
}
