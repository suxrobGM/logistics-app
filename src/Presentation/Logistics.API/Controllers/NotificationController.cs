using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.Integrations.UpdateNotifications.Commands;
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

    [HttpPut("{id:guid}", Name = "UpdateNotification")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Notification.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNotificationCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
