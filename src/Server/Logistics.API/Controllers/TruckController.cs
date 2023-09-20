using Logistics.Models;

namespace Logistics.API.Controllers;

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
    [ProducesResponseType(typeof(ResponseResult<TruckDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Truck.View)]
    public async Task<IActionResult> GetById(string id, bool includeLoads = false)
    {
        var result = await _mediator.Send(new GetTruckByIdQuery
        {
            IncludeLoads = includeLoads,
            Id = id
        });

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpGet("driver/{userId}")]
    [ProducesResponseType(typeof(ResponseResult<TruckDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Truck.View)]
    public async Task<IActionResult> GetByDriverId(string userId, [FromQuery] GetTruckByDriverQuery query)
    {
        query.UserId = userId;
        var result = await _mediator.Send(query);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(PagedResponseResult<TruckDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Truck.View)]
    public async Task<IActionResult> GetList([FromQuery] GetTrucksQuery query)
    {
        var result = await _mediator.Send(query);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
    
    [HttpGet("drivers")]
    [ProducesResponseType(typeof(PagedResponseResult<TruckDriversDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Truck.View)]
    public async Task<IActionResult> GetTruckDrivers([FromQuery] GetTruckDriversQuery query)
    {
        var result = await _mediator.Send(query);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Truck.Create)]
    public async Task<IActionResult> Create([FromBody] CreateTruckCommand request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPut("update/{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
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
