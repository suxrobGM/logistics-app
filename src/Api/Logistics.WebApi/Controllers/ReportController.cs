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

    [HttpGet("getDailyGrosses")]
    [ProducesResponseType(typeof(DataResult<DailyGrossesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Policies.Employee.CanRead)]
    public async Task<IActionResult> GetDailyGrosses([FromQuery] GetDailyGrossesQuery request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
    
    [HttpGet("getMonthlyGrosses")]
    [ProducesResponseType(typeof(DataResult<MonthlyGrossesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Policies.Employee.CanRead)]
    public async Task<IActionResult> GetMonthlyGrosses([FromQuery] GetMonthlyGrossesQuery request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
    
    [HttpGet("getTruckGrosses")]
    [ProducesResponseType(typeof(DataResult<TruckGrossesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Policies.Employee.CanRead)]
    public async Task<IActionResult> GetGrossesForTruck([FromQuery] GetTruckGrossesQuery request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
    
    [HttpGet("getOverallStats")]
    [ProducesResponseType(typeof(DataResult<OverallStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Policies.Employee.CanRead)]
    public async Task<IActionResult> GetOverallStats([FromQuery] GetOverallStatsQuery request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
}
