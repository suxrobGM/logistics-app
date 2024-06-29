using Logistics.Application.Tenant.Commands;
using Logistics.Application.Tenant.Queries;
using Logistics.Shared;
using Logistics.Shared.Models;
using Logistics.Shared.Consts.Policies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType(typeof(Result<NotificationDto[]>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Notifications.View)]
    public async Task<IActionResult> GetList([FromQuery] GetNotificationsQuery request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Notifications.Edit)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateNotificationCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
