using Logistics.Shared.Models;

namespace Logistics.API.Controllers;

[Route("trucks")]
[ApiController]
public class TrucksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrucksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{truckOrDriverId}")]
    [ProducesResponseType(typeof(ResponseResult<TruckDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Trucks.View)]
    public async Task<IActionResult> GetById(string truckOrDriverId, [FromQuery] GetTruckQuery request)
    {
        request.TruckOrDriverId = truckOrDriverId;
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseResult<TruckDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Trucks.View)]
    public async Task<IActionResult> GetList([FromQuery] GetTrucksQuery query)
    {
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
    
    [HttpGet("drivers")]
    [ProducesResponseType(typeof(PagedResponseResult<TruckDriversDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Trucks.View)]
    public async Task<IActionResult> GetTruckDrivers([FromQuery] GetTruckDriversQuery query)
    {
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Trucks.Create)]
    public async Task<IActionResult> Create([FromBody] CreateTruckCommand request)
    {
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Trucks.Edit)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateTruckCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Trucks.Delete)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteTruckCommand
        {
            Id = id
        });

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
}
