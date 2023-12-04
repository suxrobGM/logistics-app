using Logistics.Shared.Models;

namespace Logistics.API.Controllers;

[Route("drivers")]
[ApiController]
public class DriversController : ControllerBase
{
    private readonly IMediator _mediator;

    public DriversController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(ResponseResult<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Drivers.View)]
    public async Task<IActionResult> GetById(string userId)
    {
        var result = await _mediator.Send(new GetEmployeeByIdQuery {UserId = userId});
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("{userId}/device-token")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Drivers.Edit)]
    public async Task<IActionResult> SetDeviceToken(string userId, [FromBody] SetDriverDeviceTokenCommand request)
    {
        request.UserId = userId;
        var result = await _mediator.Send(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("confirm-load-status")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Drivers.Edit)]
    public async Task<IActionResult> ConfirmLoadStatus([FromBody] ConfirmLoadStatusCommand request)
    {
        var result = await _mediator.Send(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("update-load-proximity")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Drivers.Edit)]
    public async Task<IActionResult> UpdateLoadProximity([FromBody] UpdateLoadProximityCommand request)
    {
        var result = await _mediator.Send(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
