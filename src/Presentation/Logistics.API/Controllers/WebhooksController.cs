using Logistics.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[Route("webhooks")]
[ApiController]
[AllowAnonymous]
public class WebhooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public WebhooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> Stripe()
    {
        var requestBodyJson = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

        var result = await _mediator.Send(new ProcessStripEventCommand
        {
            RequestBodyJson = requestBodyJson,
            StripeSignature = stripeSignature,
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
