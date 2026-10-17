using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Logistics.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.Platform.Notifications.Commands;
using Logistics.Application.Modules.Platform.Notifications.Queries;

namespace Logistics.API.Controllers;

[ApiController]
[Route("notifications")]
[Produces("application/json")]
public class NotificationController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetNotifications")]
    [ProducesResponseType(typeof(NotificationDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Notification.View)]
    public async Task<IActionResult> GetList([FromQuery] GetNotificationsQuery request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("read", Name = "MarkNotificationsRead")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Notification.Manage)]
    public async Task<IActionResult> MarkRead([FromBody] MarkNotificationsReadCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
