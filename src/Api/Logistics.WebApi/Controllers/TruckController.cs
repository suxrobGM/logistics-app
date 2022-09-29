namespace Logistics.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class TruckController : ControllerBase
{
    private readonly IMediator _mediator;

    public TruckController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DataResult<TruckDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Truck.View)]
    public async Task<IActionResult> GetById(string id, bool includeLoadIds = false)
    {
        var result = await _mediator.Send(new GetTruckByIdQuery
        {
            Id = id
        });

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(PagedDataResult<TruckDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Truck.View)]
    public async Task<IActionResult> GetList([FromQuery] GetTrucksQuery request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Truck.Create)]
    public async Task<IActionResult> Create([FromBody] CreateTruckCommand request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPut("update/{id}")]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Truck.Edit)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateTruckCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpDelete("delete/{id}")]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Truck.Delete)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteTruckCommand
        {
            Id = id
        });

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}
