using Logistics.Shared.Models;

namespace Logistics.API.Controllers;

[Route("payments")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseResult<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.View)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery {Id = id});

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result.Error);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseResult<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.View)]
    public async Task<IActionResult> GetList([FromQuery] GetPaymentsQuery query)
    {
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentCommand request)
    {
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Edit)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePaymentCommand request)
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
    [Authorize(Policy = Permissions.Payments.Delete)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteCustomerCommand {Id = id});

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
}
