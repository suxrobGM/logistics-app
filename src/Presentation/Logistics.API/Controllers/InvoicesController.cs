using Logistics.API.Extensions;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Logistics.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.Financial.Invoices.Commands;
using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Application.Modules.Financial.PaymentLinks.Commands;
using Logistics.Application.Modules.Financial.Payments.Commands;

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
        return result.ToActionResult();
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
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}", Name = "UpdateInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInvoiceCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}", Name = "DeleteInvoice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteInvoiceCommand { Id = id });
        return result.ToActionResult();
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

        return result.ToFileResult(x => File(x.PdfBytes, "application/pdf", x.FileName));
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
        return result.ToActionResult();
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
            InvoiceId = id,
            RecipientEmail = request.Email,
            PersonalMessage = request.PersonalMessage
        });
        return result.ToActionResult();
    }

    #endregion

    #region Tax preview

    /// <summary>
    ///     Compute tax for a hypothetical set of line items without persisting an invoice.
    ///     Backs the create/edit-invoice form's live recalc.
    /// </summary>
    [HttpPost("preview-tax", Name = "PreviewInvoiceTax")]
    [ProducesResponseType(typeof(PreviewInvoiceTaxResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.View)]
    public async Task<IActionResult> PreviewTax([FromBody] PreviewInvoiceTaxRequest request)
    {
        var result = await mediator.Send(new PreviewInvoiceTaxQuery { Request = request });
        return result.ToActionResult();
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
        return result.ToActionResult();
    }

    /// <summary>
    ///     Update a line item on an invoice.
    /// </summary>
    [HttpPut("{invoiceId:guid}/line-items/{lineItemId:guid}", Name = "UpdateLineItem")]
    [ProducesResponseType(typeof(InvoiceLineItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> UpdateLineItem(Guid invoiceId, Guid lineItemId, [FromBody] UpdateLineItemRequest request)
    {
        var result = await mediator.Send(new UpdateLineItemCommand
        {
            InvoiceId = invoiceId,
            LineItemId = lineItemId,
            Description = request.Description,
            Type = request.Type,
            Amount = request.Amount,
            Quantity = request.Quantity,
            Notes = request.Notes
        });
        return result.ToActionResult();
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
        return result.ToActionResult();
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
        return result.ToActionResult();
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
        return result.ToActionResult();
    }

    #endregion
}
