using Logistics.Application.Tenant.Queries.GetDriverDashboardData;
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
    [ProducesResponseType(typeof(ResponseResult<DailyGrossesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.View)]
    public async Task<IActionResult> GetDriverDashboardData(string userId)
    {
        var result = await _mediator.Send(new GetDriverDashboardDataQuery() {UserId = userId});

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
}
