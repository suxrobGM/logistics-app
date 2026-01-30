using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("safety/behavior")]
[Produces("application/json")]
public class DriverBehaviorController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get driver behavior events with optional filters
    /// </summary>
    [HttpGet(Name = "GetDriverBehaviorEvents")]
    [ProducesResponseType(typeof(PagedResponse<DriverBehaviorEventDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Safety.View)]
    public async Task<IActionResult> GetEvents([FromQuery] GetDriverBehaviorEventsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<DriverBehaviorEventDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    /// <summary>
    /// Get driver behavior event by ID
    /// </summary>
    [HttpGet("{id:guid}", Name = "GetDriverBehaviorEventById")]
    [ProducesResponseType(typeof(DriverBehaviorEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Safety.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetDriverBehaviorEventByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Review and mark a driver behavior event as reviewed
    /// </summary>
    [HttpPost("{id:guid}/review", Name = "ReviewDriverBehaviorEvent")]
    [ProducesResponseType(typeof(DriverBehaviorEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Safety.Manage)]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewDriverBehaviorEventCommand request)
    {
        request = request with { Id = id };
        var result = await mediator.Send(request);

        if (!result.IsSuccess)
        {
            return result.Error?.Contains("not found") == true
                ? NotFound(ErrorResponse.FromResult(result))
                : BadRequest(ErrorResponse.FromResult(result));
        }

        return Ok(result.Value);
    }
}
