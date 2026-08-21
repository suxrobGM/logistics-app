using Logistics.Application.Modules.Integrations.Negotiation.Commands;
using Logistics.Application.Modules.Integrations.Negotiation.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

/// <summary>
/// Read and close broker rate negotiations. Sending a counter-offer is deliberately absent:
/// offers only leave through an approved agent decision.
/// </summary>
[ApiController]
[Route("negotiations")]
[Produces("application/json")]
public class NegotiationController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetNegotiations")]
    [ProducesResponseType(typeof(PagedResponse<RateNegotiationDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Negotiation.View)]
    public async Task<IActionResult> GetNegotiations([FromQuery] GetNegotiationsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<RateNegotiationDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpGet("{id:guid}", Name = "GetNegotiationById")]
    [ProducesResponseType(typeof(RateNegotiationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Negotiation.View)]
    public async Task<IActionResult> GetNegotiationById(Guid id)
    {
        var result = await mediator.Send(new GetNegotiationByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet("decisions/{decisionId:guid}/preview", Name = "PreviewCounterOffer")]
    [ProducesResponseType(typeof(CounterOfferPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Negotiation.View)]
    public async Task<IActionResult> PreviewCounterOffer(Guid decisionId)
    {
        var result = await mediator.Send(new PreviewCounterOfferQuery { DecisionId = decisionId });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("{id:guid}/close", Name = "CloseNegotiation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Negotiation.Manage)]
    public async Task<IActionResult> CloseNegotiation(Guid id, [FromBody] CloseNegotiationCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
