using Logistics.Models;

namespace Logistics.API.Controllers;

[Route("[controller]")]
[ApiController]
public class DriverController : ControllerBase
{
    private readonly IMediator _mediator;

    public DriverController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(ResponseResult<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.View)]
    public async Task<IActionResult> GetById(string userId)
    {
        var result = await _mediator.Send(new GetEmployeeByIdQuery {UserId = userId});

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }

    [HttpGet("{userId}/activeLoads")]
    [ProducesResponseType(typeof(ResponseResult<DriverActiveLoadsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.View)]
    public async Task<IActionResult> GetDriverActiveLoads(string userId)
    {
        var result = await _mediator.Send(new GetDriverActiveLoadsQuery {UserId = userId});

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
    
    [HttpGet("{userId}/truck")]
    [ProducesResponseType(typeof(ResponseResult<TruckDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Truck.View)]
    public async Task<IActionResult> GetTruckByDriver(string userId, [FromQuery] GetTruckByDriverQuery query)
    {
        query.UserId = userId;
        var result = await _mediator.Send(query);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
    
    [HttpPost("{userId}/deviceToken")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.View)]
    public async Task<IActionResult> SetDeviceToken(string userId, [FromBody] SetDriverDeviceTokenCommand command)
    {
        command.UserId = userId;
        var result = await _mediator.Send(command);

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
}
