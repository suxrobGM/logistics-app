using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("payments")]
public class PaymentController(IMediator mediator) : ControllerBase
{
    #region Payments

    [HttpGet("{id:guid}", Name = "GetPaymentById")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permissions.Payments.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetPaymentQuery { Id = id });
        return result.Success ? Ok(result.Data) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetPayments")]
    [ProducesResponseType(typeof(PagedResponse<PaymentDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permissions.Payments.View)]
    public async Task<IActionResult> GetList([FromQuery] GetPaymentsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<PaymentDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpPost(Name = "CreatePayment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentCommand request)
    {
        var result = await mediator.Send(request);
        return result.Success ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("process-payment", Name = "ProcessPayment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand request)
    {
        var result = await mediator.Send(request);
        return result.Success ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdatePayment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.Success ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeletePayment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permissions.Payments.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteCustomerCommand { Id = id });
        return result.Success ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    #endregion


    #region Payment Methods

    [HttpGet("methods/{id:guid}", Name = "GetPaymentMethodById")]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permissions.Payments.View)]
    public async Task<IActionResult> GetPaymentMethodById(Guid id)
    {
        var result = await mediator.Send(new GetPaymentMethodQuery { Id = id });
        return result.Success ? Ok(result.Data) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet("methods", Name = "GetPaymentMethods")]
    [ProducesResponseType(typeof(PaymentMethodDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.View)]
    public async Task<IActionResult> GetPaymentMethods([FromQuery] GetPaymentMethodsQuery query)
    {
        var result = await mediator.Send(query);
        return result.Success ? Ok(result.Data) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("methods", Name = "CreatePaymentMethod")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Create)]
    public async Task<IActionResult> CreatePaymentMethod([FromBody] CreatePaymentMethodCommand request)
    {
        var result = await mediator.Send(request);
        return result.Success ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("methods/setup-intent", Name = "CreateSetupIntent")]
    [ProducesResponseType(typeof(SetupIntentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Create)]
    public async Task<IActionResult> CreateSetupIntent()
    {
        var result = await mediator.Send(new CreateSetupIntentCommand());
        return result.Success ? Ok(result.Data) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("methods", Name = "UpdatePaymentMethod")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Edit)]
    public async Task<IActionResult> UpdatePaymentMethod([FromBody] UpdatePaymentMethodCommand request)
    {
        var result = await mediator.Send(request);
        return result.Success ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("methods/default", Name = "SetDefaultPaymentMethod")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Edit)]
    public async Task<IActionResult> SetDefaultPaymentMethod([FromBody] SetDefaultPaymentMethodCommand request)
    {
        var result = await mediator.Send(request);
        return result.Success ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("methods/{id:guid}", Name = "DeletePaymentMethod")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permissions.Payments.Edit)]
    public async Task<IActionResult> DeletePaymentMethod(Guid id)
    {
        var result = await mediator.Send(new DeletePaymentMethodCommand { Id = id });
        return result.Success ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    #endregion
}
