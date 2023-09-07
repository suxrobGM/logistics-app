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

    [HttpGet("{userId}/dashboard")]
    [ProducesResponseType(typeof(ResponseResult<DriverDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.View)]
    public async Task<IActionResult> GetDriverDashboardData(string userId)
    {
        var result = await _mediator.Send(new GetDriverDashboardDataQuery() {UserId = userId});

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
    
    [HttpPost("{userId}/deviceToken")]
    [ProducesResponseType(typeof(ResponseResult<DriverDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.View)]
    public async Task<IActionResult> SetDriverDeviceToken([FromBody] SetDriverDeviceTokenCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
    
    [HttpGet("list")]
    [ProducesResponseType(typeof(PagedResponseResult<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.View)]
    public async Task<IActionResult> GetDriversList([FromQuery] GetDriversQuery query)
    {
        var result = await _mediator.Send(query);

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
}
