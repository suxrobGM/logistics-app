using Logistics.Domain.Primitives.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.Compliance.Eld.Commands;
using Logistics.Application.Modules.Integrations.Webhooks.Commands;

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

    [HttpPost("eld/tteld", Name = "ProcessTtEldWebhook")]
    public Task<IActionResult> TtEldWebhook()
    {
        // TT ELD does not support webhooks - GPS data is polled via EldSyncJob
        return Task.FromResult<IActionResult>(Ok());
    }

    [HttpPost("eld/geotab", Name = "ProcessGeotabWebhook")]
    public async Task<IActionResult> GeotabWebhook()
    {
        var requestBodyJson = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["X-Geotab-Signature"].ToString();

        var result = await mediator.Send(new ProcessEldWebhookCommand
        {
            ProviderType = EldProviderType.Geotab,
            RequestBodyJson = requestBodyJson,
            Signature = signature
        });
        return result.IsSuccess ? Ok() : BadRequest();
    }

    #region Load Board Webhooks

    [HttpPost("loadboard/dat", Name = "ProcessDatWebhook")]
    public Task<IActionResult> DatWebhook()
    {
        // TODO: Implement DAT webhook processing when provider is implemented
        return Task.FromResult<IActionResult>(Ok());
    }

    [HttpPost("loadboard/truckstop", Name = "ProcessTruckstopWebhook")]
    public Task<IActionResult> TruckstopWebhook()
    {
        // TODO: Implement Truckstop webhook processing when provider is implemented
        return Task.FromResult<IActionResult>(Ok());
    }

    [HttpPost("loadboard/123loadboard", Name = "Process123LoadboardWebhook")]
    public Task<IActionResult> OneTwo3LoadboardWebhook()
    {
        // TODO: Implement 123Loadboard webhook processing when provider is implemented
        return Task.FromResult<IActionResult>(Ok());
    }

    #endregion
}
