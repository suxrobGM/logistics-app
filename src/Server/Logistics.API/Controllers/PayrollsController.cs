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
        var result = await _mediator.Send(new GetPaymentByIdQuery {Id = id});

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result.Error);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseResult<PayrollDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payrolls.View)]
    public async Task<IActionResult> GetList([FromQuery] GetPayrollsQuery query)
    {
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
}
