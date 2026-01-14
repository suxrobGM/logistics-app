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
[Produces("application/json")]
public class TripController(IMediator mediator) : ControllerBase
{
    [HttpGet("{tripId:guid}", Name = "GetTripById")]
    [ProducesResponseType(typeof(TripDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Load.View)]
    public async Task<IActionResult> GetById(Guid tripId)
    {
        var result = await mediator.Send(new GetTripQuery { TripId = tripId });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetTrips")]
    [ProducesResponseType(typeof(PagedResponse<TripDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Load.View)]
    public async Task<IActionResult> GetList([FromQuery] GetTripsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<TripDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpPost(Name = "CreateTrip")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Load.Manage)]
    public async Task<IActionResult> CreateTrip([FromBody] CreateTripCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("optimize", Name = "OptimizeTripStops")]
    [ProducesResponseType(typeof(OptimizedTripStopsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Load.View)]
    public async Task<IActionResult> OptimizeTripStops([FromBody] OptimizeTripStopsCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateTrip")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Load.Manage)]
    public async Task<IActionResult> UpdateTrip(Guid id, [FromBody] UpdateTripCommand request)
    {
        request.TripId = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteTrip")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Load.Manage)]
    public async Task<IActionResult> DeleteTrip(Guid id)
    {
        var result = await mediator.Send(new DeleteTripCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }
}
