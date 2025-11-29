using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("trips")]
public class TripController(IMediator mediator) : ControllerBase
{
    [HttpGet("{tripId:guid}", Name = "GetTripById")]
    [ProducesResponseType(typeof(Result<TripDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.View)]
    public async Task<IActionResult> GetById(Guid tripId)
    {
        var result = await mediator.Send(new GetTripQuery { TripId = tripId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet(Name = "GetTrips")]
    [ProducesResponseType(typeof(PagedResult<TripDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.View)]
    public async Task<IActionResult> GetList([FromQuery] GetTripsQuery query)
    {
        var result = await mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost(Name = "CreateTrip")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.Create)]
    public async Task<IActionResult> CreateTrip([FromBody] CreateTripCommand request)
    {
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("optimize", Name = "OptimizeTripStops")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.View)]
    public async Task<IActionResult> OptimizeTripStops([FromBody] OptimizeTripStopsCommand request)
    {
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}", Name = "UpdateTrip")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.Edit)]
    public async Task<IActionResult> UpdateTrip(Guid id, [FromBody] UpdateTripCommand request)
    {
        request.TripId = id;
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}", Name = "DeleteTrip")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.Delete)]
    public async Task<IActionResult> DeleteTrip(Guid id)
    {
        var result = await mediator.Send(new DeleteTripCommand { Id = id });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
