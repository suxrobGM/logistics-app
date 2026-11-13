using Logistics.API.Extensions;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Logistics.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.Financial.Payments.Commands;
using Logistics.Application.Modules.Financial.Payments.Queries;

namespace Logistics.API.Controllers.Financial;

[ApiController]
[Route("payments")]
[Produces("application/json")]
public class PaymentController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "GetPaymentById")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Payment.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetPaymentQuery { Id = id });
        return result.ToActionResult();
    }

    [HttpGet(Name = "GetPayments")]
    [ProducesResponseType(typeof(PagedResponse<PaymentDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Payment.View)]
    public async Task<IActionResult> GetList([FromQuery] GetPaymentsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<PaymentDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpPost(Name = "CreatePayment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payment.Manage)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPost("process-payment", Name = "ProcessPayment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payment.Manage)]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}", Name = "UpdatePayment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payment.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}", Name = "DeletePayment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Payment.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeletePaymentCommand { Id = id });
        return result.ToActionResult();
    }

    [HttpPost("methods/setup-intent", Name = "CreateSetupIntent")]
    [ProducesResponseType(typeof(SetupIntentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payment.Manage)]
    public async Task<IActionResult> CreateSetupIntent()
    {
        var result = await mediator.Send(new CreateSetupIntentCommand());
        return result.ToActionResult();
    }
}
