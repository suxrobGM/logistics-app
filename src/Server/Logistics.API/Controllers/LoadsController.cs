using Logistics.Shared.Models;

namespace Logistics.API.Controllers;

[Route("loads")]
[ApiController]
public class LoadsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LoadsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseResult<LoadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.View)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _mediator.Send(new GetLoadByIdQuery
        {
            Id = id
        });

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
    
    

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseResult<LoadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.View)]
    public async Task<IActionResult> GetList([FromQuery] GetLoadsQuery query)
    {
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.Create)]
    public async Task<IActionResult> Create([FromBody] CreateLoadCommand request)
    {
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.Edit)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateLoadCommand request)
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
    [Authorize(Policy = Permissions.Loads.Delete)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteLoadCommand
        {
            Id = id
        });

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
}
