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

    /// <summary>
    ///     Get invoice dashboard statistics.
    /// </summary>
    [HttpGet("dashboard", Name = "GetInvoiceDashboard")]
    [ProducesResponseType(typeof(InvoiceDashboardDto), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Invoice.View)]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await mediator.Send(new GetInvoiceDashboardQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
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

    /// <summary>
    ///     Download a load invoice as PDF.
    /// </summary>
    [HttpGet("loads/{id:guid}/pdf", Name = "DownloadLoadInvoicePdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Invoice.View)]
    public async Task<IActionResult> DownloadLoadInvoicePdf(Guid id)
    {
        var result = await mediator.Send(new GetLoadInvoicePdfQuery { InvoiceId = id });

        if (!result.IsSuccess)
        {
            return NotFound(ErrorResponse.FromResult(result));
        }

        return File(result.Value!.PdfBytes, "application/pdf", result.Value.FileName);
    }

    #endregion

    #region Manual Payments

    /// <summary>
    ///     Record a manual payment (cash or check) for an invoice.
    ///     Only Owner and Manager roles can record manual payments.
    /// </summary>
    [HttpPost("{id:guid}/payments/manual", Name = "RecordManualPayment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> RecordManualPayment(Guid id, [FromBody] RecordManualPaymentRequest request)
    {
        var result = await mediator.Send(new RecordManualPaymentCommand
        {
            InvoiceId = id,
            Amount = request.Amount,
            Type = request.Type,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            ReceivedDate = request.ReceivedDate
        });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Send Invoice

    /// <summary>
    ///     Send an invoice to a customer via email with a payment link.
    /// </summary>
    [HttpPost("{id:guid}/send", Name = "SendInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> SendInvoice(Guid id, [FromBody] SendInvoiceRequest request)
    {
        var result = await mediator.Send(new SendInvoiceCommand
        {
            InvoiceId = id, RecipientEmail = request.Email, PersonalMessage = request.PersonalMessage
        });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Line Items

    /// <summary>
    ///     Add a line item to an invoice.
    /// </summary>
    [HttpPost("{id:guid}/line-items", Name = "AddLineItem")]
    [ProducesResponseType(typeof(InvoiceLineItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> AddLineItem(Guid id, [FromBody] AddLineItemRequest request)
    {
        var result = await mediator.Send(new AddLineItemCommand
        {
            InvoiceId = id,
            Description = request.Description,
            Type = request.Type,
            Amount = request.Amount,
            Quantity = request.Quantity,
            Notes = request.Notes
        });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Remove a line item from an invoice.
    /// </summary>
    [HttpDelete("{invoiceId:guid}/line-items/{lineItemId:guid}", Name = "DeleteLineItem")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> DeleteLineItem(Guid invoiceId, Guid lineItemId)
    {
        var result = await mediator.Send(new DeleteLineItemCommand(invoiceId, lineItemId));
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Payment Links

    /// <summary>
    ///     Create a new payment link for an invoice.
    /// </summary>
    [HttpPost("{id:guid}/payment-links", Name = "CreatePaymentLink")]
    [ProducesResponseType(typeof(PaymentLinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> CreatePaymentLink(Guid id, [FromBody] CreatePaymentLinkRequest? request = null)
    {
        var command = new CreatePaymentLinkCommand { InvoiceId = id };
        if (request?.ExpiresInDays.HasValue == true)
        {
            command.ExpirationDays = request.ExpiresInDays.Value;
        }

        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Revoke a payment link.
    /// </summary>
    [HttpDelete("{invoiceId:guid}/payment-links/{linkId:guid}", Name = "RevokePaymentLink")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> RevokePaymentLink(Guid invoiceId, Guid linkId)
    {
        var result = await mediator.Send(new RevokePaymentLinkCommand(linkId));
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
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

    /// <summary>
    ///     Submit a payroll invoice for approval.
    /// </summary>
    [HttpPost("payrolls/{id:guid}/submit", Name = "SubmitPayrollForApproval")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> SubmitPayrollForApproval(Guid id)
    {
        var result = await mediator.Send(new SubmitPayrollForApprovalCommand { Id = id });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Approve a payroll invoice.
    /// </summary>
    [HttpPost("payrolls/{id:guid}/approve", Name = "ApprovePayrollInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Approve)]
    public async Task<IActionResult> ApprovePayrollInvoice(Guid id, [FromBody] ApprovePayrollRequest? request = null)
    {
        var result = await mediator.Send(new ApprovePayrollInvoiceCommand
        {
            Id = id,
            Notes = request?.Notes
        });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Reject a payroll invoice.
    /// </summary>
    [HttpPost("payrolls/{id:guid}/reject", Name = "RejectPayrollInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Approve)]
    public async Task<IActionResult> RejectPayrollInvoice(Guid id, [FromBody] RejectPayrollRequest request)
    {
        var result = await mediator.Send(new RejectPayrollInvoiceCommand
        {
            Id = id,
            Reason = request.Reason
        });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Batch approve multiple payroll invoices.
    /// </summary>
    [HttpPost("payrolls/batch-approve", Name = "BatchApprovePayroll")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Approve)]
    public async Task<IActionResult> BatchApprovePayroll([FromBody] BatchApprovePayrollRequest request)
    {
        var result = await mediator.Send(new BatchApprovePayrollCommand
        {
            Ids = request.Ids,
            Notes = request.Notes
        });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion
}
