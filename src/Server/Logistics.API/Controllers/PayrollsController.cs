using Logistics.Shared.Models;

namespace Logistics.API.Controllers;

[Route("payrolls")]
[ApiController]
public class PayrollsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PayrollsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseResult<PayrollDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payrolls.View)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _mediator.Send(new GetPayrollByIdQuery {Id = id});
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    
    [HttpGet("calculate")]
    [ProducesResponseType(typeof(ResponseResult<PayrollDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payrolls.View)]
    public async Task<IActionResult> CalculateEmployeePayroll([FromQuery] CalculatePayrollQuery request)
    {
        var result = await _mediator.Send(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseResult<PayrollDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payrolls.View)]
    public async Task<IActionResult> GetList([FromQuery] GetPayrollsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payrolls.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePayrollCommand request)
    {
        var result = await _mediator.Send(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payrolls.Edit)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePayrollCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payrolls.Delete)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _mediator.Send(new DeletePayrollCommand {Id = id});
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
