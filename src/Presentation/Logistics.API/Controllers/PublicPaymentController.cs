using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.Financial.PaymentLinks.Queries;
using Logistics.Application.Modules.Financial.Payments.Commands;

namespace Logistics.API.Controllers;

/// <summary>
/// API endpoints for public payment links (no authentication required).
/// These endpoints allow customers to view invoices and pay them via Stripe's hosted Checkout
/// page from a shareable link.
/// </summary>
[ApiController]
[Route("public/payments")]
[Produces("application/json")]
[AllowAnonymous]
public class PublicPaymentController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get invoice details via a public payment link.
    /// </summary>
    [HttpGet("{tenantId:guid}/{token}", Name = "GetPublicInvoice")]
    [ProducesResponseType(typeof(PublicInvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoice(Guid tenantId, string token)
    {
        var result = await mediator.Send(new GetPublicInvoiceQuery(tenantId, token));
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Creates a Stripe Checkout Session for the invoice and returns the hosted-page URL.
    /// The customer is redirected to the URL to complete payment.
    /// </summary>
    [HttpPost("{tenantId:guid}/{token}/checkout", Name = "CreatePublicCheckoutSession")]
    [ProducesResponseType(typeof(PublicCheckoutSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCheckoutSession(
        Guid tenantId,
        string token,
        [FromBody] CreatePublicCheckoutSessionRequest request)
    {
        var result = await mediator.Send(new CreatePublicCheckoutSessionCommand
        {
            TenantId = tenantId,
            Token = token,
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            Amount = request.Amount
        });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }
}

public record CreatePublicCheckoutSessionRequest
{
    public required string SuccessUrl { get; set; }
    public required string CancelUrl { get; set; }
    public decimal? Amount { get; set; }
}
