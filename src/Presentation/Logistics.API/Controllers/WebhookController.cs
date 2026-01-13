using Logistics.Application.Commands;
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
}
