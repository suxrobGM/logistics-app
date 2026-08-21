using Logistics.Application.Modules.Integrations.Negotiation.Commands;
using Logistics.Application.Modules.Integrations.Negotiation.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("ratefloors")]
[Produces("application/json")]
public class RateFloorController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetLaneRateFloors")]
    [ProducesResponseType(typeof(List<LaneRateFloorDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Negotiation.View)]
    public async Task<IActionResult> GetLaneRateFloors()
    {
        var result = await mediator.Send(new GetLaneRateFloorsQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("effective/{listingId:guid}", Name = "GetEffectiveRateFloor")]
    [ProducesResponseType(typeof(EffectiveRateFloorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Negotiation.View)]
    public async Task<IActionResult> GetEffectiveRateFloor(Guid listingId)
    {
        var result = await mediator.Send(new GetEffectiveRateFloorQuery { ListingId = listingId });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost(Name = "CreateLaneRateFloor")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Negotiation.Manage)]
    public async Task<IActionResult> CreateLaneRateFloor([FromBody] CreateLaneRateFloorCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateLaneRateFloor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Negotiation.Manage)]
    public async Task<IActionResult> UpdateLaneRateFloor(Guid id, [FromBody] UpdateLaneRateFloorCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteLaneRateFloor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Negotiation.Manage)]
    public async Task<IActionResult> DeleteLaneRateFloor(Guid id)
    {
        var result = await mediator.Send(new DeleteLaneRateFloorCommand { Id = id });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
