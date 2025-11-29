using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("notifications")]
public class NotificationController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetNotifications")]
    [ProducesResponseType(typeof(Result<NotificationDto[]>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Notifications.View)]
    public async Task<IActionResult> GetList([FromQuery] GetNotificationsQuery request)
    {
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}", Name = "UpdateNotification")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Notifications.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNotificationCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
