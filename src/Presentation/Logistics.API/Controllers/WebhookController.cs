using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.Compliance.Eld.Commands;
using Logistics.Application.Modules.Integrations.Webhooks.Commands;
using Logistics.Shared.Models;

namespace Logistics.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("webhooks")]
[Produces("application/json")]
public class WebhookController(IMediator mediator) : ControllerBase
{
    [HttpPost("stripe", Name = "ProcessStripeWebhook")]
    public async Task<IActionResult> Stripe()
    {
        var requestBodyJson = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

        var result = await mediator.Send(new ProcessStripEventCommand
        {
            RequestBodyJson = requestBodyJson,
            StripeSignature = stripeSignature
        });
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Inbound mail from Resend. A 500 is deliberate on a transient failure - it is what makes
    /// Resend retry a delivery we could not finish.
    /// </summary>
    [HttpPost("resend", Name = "ProcessResendWebhook")]
    [RequestSizeLimit(512 * 1024)]
    public async Task<IActionResult> Resend()
    {
        var requestBodyJson = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        var result = await mediator.Send(new ProcessResendWebhookCommand
        {
            RawBody = requestBodyJson,
            SvixId = Request.Headers["svix-id"].ToString(),
            SvixTimestamp = Request.Headers["svix-timestamp"].ToString(),
            SvixSignature = Request.Headers["svix-signature"].ToString()
        });

        if (result.IsSuccess)
        {
            return Ok();
        }

        return result.ErrorCode == ErrorCodes.WebhookRejected
            ? BadRequest()
            : StatusCode(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// ELD webhooks are registered per tenant (the URL is shown on the provider settings page), because the
    /// endpoint is anonymous and the signature can only be checked against the right tenant's secret.
    /// </summary>
    [HttpPost("eld/samsara/{tenantId:guid}", Name = "ProcessSamsaraWebhook")]
    public Task<IActionResult> SamsaraWebhook(Guid tenantId) =>
        ProcessEldWebhookAsync(tenantId, EldProviderType.Samsara, "X-Samsara-Signature", "X-Samsara-Timestamp");

    [HttpPost("eld/motive/{tenantId:guid}", Name = "ProcessMotiveWebhook")]
    public Task<IActionResult> MotiveWebhook(Guid tenantId) =>
        ProcessEldWebhookAsync(tenantId, EldProviderType.Motive, "X-Motive-Signature");

    [HttpPost("eld/geotab/{tenantId:guid}", Name = "ProcessGeotabWebhook")]
    public Task<IActionResult> GeotabWebhook(Guid tenantId) =>
        ProcessEldWebhookAsync(tenantId, EldProviderType.Geotab, "X-Geotab-Signature");

    private async Task<IActionResult> ProcessEldWebhookAsync(
        Guid tenantId, EldProviderType providerType, string signatureHeader, string? timestampHeader = null)
    {
        var requestBodyJson = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        var result = await mediator.Send(new ProcessEldWebhookCommand
        {
            TenantId = tenantId,
            ProviderType = providerType,
            RequestBodyJson = requestBodyJson,
            Signature = Request.Headers[signatureHeader].ToString(),
            Timestamp = timestampHeader is null ? null : Request.Headers[timestampHeader].ToString()
        });
        return result.IsSuccess ? Ok() : BadRequest();
    }
}
