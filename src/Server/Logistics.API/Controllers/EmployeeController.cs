using Logistics.Models;

namespace Logistics.API.Controllers;

[Route("[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(ResponseResult<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.View)]
    public async Task<IActionResult> GetById(string userId)
    {
        var result = await _mediator.Send(new GetEmployeeByIdQuery
        {
            UserId = userId
        });

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(PagedResponseResult<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.View)]
    public async Task<IActionResult> GetList([FromQuery] GetEmployeesQuery query)
    {
        var result = await _mediator.Send(query);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.Create)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeCommand request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
    
    [HttpPost("removeRole")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.Edit)]
    public async Task<IActionResult> RemoveRole([FromBody] RemoveRoleFromEmployeeCommand request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPut("update/{userId}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.Edit)]
    public async Task<IActionResult> Update(string userId, [FromBody] UpdateEmployeeCommand request)
    {
        request.UserId = userId;
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
    
    [HttpDelete("delete/{userId}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employee.Delete)]
    public async Task<IActionResult> Delete(string userId)
    {
        var result = await _mediator.Send(new DeleteEmployeeCommand
        {
            UserId = userId
        });

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}
