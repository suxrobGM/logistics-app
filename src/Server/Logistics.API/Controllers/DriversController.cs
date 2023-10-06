using Logistics.API.Hubs;
using Logistics.Models;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.API.Controllers;

[Route("drivers")]
[ApiController]
public class DriversController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<NotificationHub, INotificationHubClient> _notificationHub;

    public DriversController(
        IMediator mediator, 
        IHubContext<NotificationHub, INotificationHubClient> notificationHub)
    {
        _mediator = mediator;
        _notificationHub = notificationHub;
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(ResponseResult<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.View)]
    public async Task<IActionResult> GetById(string userId)
    {
        var result = await _mediator.Send(new GetEmployeeByIdQuery {UserId = userId});

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result.Error);
    }
    
    [HttpPost("{userId}/device-token")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.View)]
    public async Task<IActionResult> SetDeviceToken(string userId, [FromBody] SetDriverDeviceTokenCommand request)
    {
        request.UserId = userId;
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result.Error);
    }
    
    [HttpPost("confirm-load-status")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.View)]
    public async Task<IActionResult> ConfirmLoadStatus([FromBody] ConfirmLoadStatusCommand request)
    {
        request.SendNotificationAsync = (tenantId, notification) =>
            _notificationHub.Clients.Group(tenantId).ReceiveNotification(notification);
        
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result.Error);
    }
    
    [HttpPost("update-load-proximity")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.View)]
    public async Task<IActionResult> UpdateLoadProximity([FromBody] UpdateLoadProximityCommand request)
    {
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result.Error);
    }
}
