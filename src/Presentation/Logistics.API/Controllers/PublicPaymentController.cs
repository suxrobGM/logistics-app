using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

/// <summary>
/// API endpoints for public payment links (no authentication required).
/// These endpoints allow customers to view invoices and make payments via shareable links.
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
    /// Process a payment via a public payment link.
    /// Creates a Stripe PaymentIntent with destination charges to the company's connected account.
    /// </summary>
    [HttpPost("{tenantId:guid}/{token}/pay", Name = "ProcessPublicPayment")]
    [ProducesResponseType(typeof(ProcessPublicPaymentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessPayment(
        Guid tenantId,
        string token,
        [FromBody] ProcessPublicPaymentRequest request)
    {
        var result = await mediator.Send(new ProcessPublicPaymentCommand
        {
            TenantId = tenantId,
            Token = token,
            Amount = request.Amount,
            StripePaymentMethodId = request.PaymentMethodId
        });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Create a Stripe SetupIntent for saving a payment method via a public payment link.
    /// </summary>
    [HttpPost("{tenantId:guid}/{token}/setup", Name = "CreatePublicSetupIntent")]
    [ProducesResponseType(typeof(SetupIntentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSetupIntent(Guid tenantId, string token)
    {
        var result = await mediator.Send(new CreatePublicSetupIntentCommand
        {
            TenantId = tenantId,
            Token = token
        });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }
}
