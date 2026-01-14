using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("invoices")]
[Produces("application/json")]
public class InvoicesController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "GetInvoiceById")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Invoice.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetInvoiceByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetInvoices")]
    [ProducesResponseType(typeof(PagedResponse<InvoiceDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Invoice.View)]
    public async Task<IActionResult> GetList([FromQuery] GetInvoicesQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<InvoiceDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpPut("{id:guid}", Name = "UpdateInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInvoiceCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteInvoiceCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    #region Load Invoice

    [HttpPost("loads", Name = "CreateLoadInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> CreateLoadInvoice([FromBody] CreateLoadInvoiceCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Payroll Invoice

    [HttpGet("payrolls/preview", Name = "PreviewPayrollInvoice")]
    [ProducesResponseType(typeof(PayrollDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.View)]
    public async Task<IActionResult> PreviewPayrollInvoice([FromQuery] PreviewPayrollInvoiceQuery request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("payrolls", Name = "CreatePayrollInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> CreatePayrollInvoice([FromBody] CreatePayrollInvoiceCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("payrolls/{id:guid}", Name = "UpdatePayrollInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> UpdatePayrollInvoice(Guid id, [FromBody] UpdatePayrollInvoiceCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion
}
