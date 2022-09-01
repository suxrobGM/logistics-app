namespace Logistics.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class GrossController : ControllerBase
{
    private readonly IMediator _mediator;

    public GrossController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("getForPeriod")]
    [ProducesResponseType(typeof(DataResult<GrossesPerDayDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Policies.Employee.CanRead)]
    public async Task<IActionResult> GetGrossesForPeriod([FromQuery] GetGrossesForPeriodQuery request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
}
