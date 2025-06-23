using Logistics.Application.Commands;
using Logistics.Shared.Models;
using Logistics.Shared.Consts.Policies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[Route("trips")]
[ApiController]
public class TripController : ControllerBase
{
    private readonly IMediator _mediator;

    public TripController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.Create)]
    public async Task<IActionResult> CreateTrip([FromBody] CreateTripCommand request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.Delete)]
    public async Task<IActionResult> DeleteTrip(Guid id)
    {
        var result = await _mediator.Send(new DeleteTripCommand {Id = id});
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
