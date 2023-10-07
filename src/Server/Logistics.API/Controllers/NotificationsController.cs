using Logistics.Shared.Models;

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
    [Authorize(Policy = Permissions.Notifications.View)]
    public async Task<IActionResult> GetList([FromQuery] GetNotificationsQuery request)
    {
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Notifications.Edit)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateNotificationCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
}
