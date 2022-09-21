namespace Logistics.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("getForInterval")]
    [ProducesResponseType(typeof(DataResult<GrossesForIntervalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Policies.Employee.CanRead)]
    public async Task<IActionResult> GetGrossesForInterval([FromQuery] GetGrossesForIntervalQuery request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
    
    [HttpGet("getForTruck")]
    [ProducesResponseType(typeof(DataResult<TruckGrossesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Policies.Employee.CanRead)]
    public async Task<IActionResult> GetGrossesForTruck([FromQuery] GetTruckGrossesForIntervalQuery request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
}
