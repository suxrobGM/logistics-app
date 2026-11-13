using Logistics.API.Extensions;
using Logistics.Application.Modules.AI.Negotiation.Commands;
using Logistics.Application.Modules.AI.Negotiation.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers.AI;

[ApiController]
[Route("ratefloors")]
[Produces("application/json")]
public class RateFloorController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetLaneRateFloors")]
    [ProducesResponseType(typeof(List<LaneRateFloorDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.AINegotiation.View)]
    public async Task<IActionResult> GetLaneRateFloors()
    {
        var result = await mediator.Send(new GetLaneRateFloorsQuery());
        return result.ToActionResult();
    }

    [HttpPost(Name = "CreateLaneRateFloor")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AINegotiation.Manage)]
    public async Task<IActionResult> CreateLaneRateFloor([FromBody] CreateLaneRateFloorCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}", Name = "UpdateLaneRateFloor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AINegotiation.Manage)]
    public async Task<IActionResult> UpdateLaneRateFloor(Guid id, [FromBody] UpdateLaneRateFloorCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}", Name = "DeleteLaneRateFloor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AINegotiation.Manage)]
    public async Task<IActionResult> DeleteLaneRateFloor(Guid id)
    {
        var result = await mediator.Send(new DeleteLaneRateFloorCommand { Id = id });
        return result.ToActionResult();
    }
}
