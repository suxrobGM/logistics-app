using Logistics.Application.Commands;
using Logistics.Domain.Primitives.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("webhooks")]
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

    [HttpPost("eld/samsara", Name = "ProcessSamsaraWebhook")]
    public async Task<IActionResult> SamsaraWebhook()
    {
        var requestBodyJson = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["X-Samsara-Signature"].ToString();

        var result = await mediator.Send(new ProcessEldWebhookCommand
        {
            ProviderType = EldProviderType.Samsara,
            RequestBodyJson = requestBodyJson,
            Signature = signature
        });
        return result.IsSuccess ? Ok() : BadRequest();
    }

    [HttpPost("eld/motive", Name = "ProcessMotiveWebhook")]
    public async Task<IActionResult> MotiveWebhook()
    {
        var requestBodyJson = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["X-Motive-Signature"].ToString();

        var result = await mediator.Send(new ProcessEldWebhookCommand
        {
            ProviderType = EldProviderType.Motive,
            RequestBodyJson = requestBodyJson,
            Signature = signature
        });
        return result.IsSuccess ? Ok() : BadRequest();
    }
}
