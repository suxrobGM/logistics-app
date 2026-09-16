using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Logistics.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.IdentityAccess.Employees.Commands;
using Logistics.Application.Modules.IdentityAccess.Employees.Queries;
using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Application.Modules.Operations.Trips.Commands;

namespace Logistics.API.Controllers;

[ApiController]
[Route("drivers")]
[Produces("application/json")]
public class DriverController(IMediator mediator) : ControllerBase
{
    [HttpGet("{userId:guid}", Name = "GetDriverByUserId")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Driver.View)]
    public async Task<IActionResult> GetById(Guid userId)
    {
        var result = await mediator.Send(new GetEmployeeByIdQuery { UserId = userId });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpPost("{userId:guid}/device-token", Name = "SetDriverDeviceToken")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Driver.Manage)]
    public async Task<IActionResult> SetDeviceToken(Guid userId, [FromBody] SetDriverDeviceTokenCommand request)
    {
        request.UserId = userId;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("confirm-load-status", Name = "ConfirmLoadStatus")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Driver.Manage)]
    public async Task<IActionResult> ConfirmLoadStatus([FromBody] ConfirmLoadStatusCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("update-load-proximity", Name = "UpdateLoadProximity")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Driver.Manage)]
    public async Task<IActionResult> UpdateLoadProximity([FromBody] UpdateLoadProximityCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("trips/{tripId:guid}/stops/{stopId:guid}/arrive", Name = "DriverMarkStopArrived")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Driver.Manage)]
    public async Task<IActionResult> MarkStopArrived(Guid tripId, Guid stopId)
    {
        var result = await mediator.Send(new MarkStopArrivedCommand { TripId = tripId, StopId = stopId });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("trips/{tripId:guid}/stops/{stopId:guid}/depart", Name = "DriverMarkStopDeparted")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Driver.Manage)]
    public async Task<IActionResult> MarkStopDeparted(Guid tripId, Guid stopId)
    {
        var result = await mediator.Send(new MarkStopDepartedCommand { TripId = tripId, StopId = stopId });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
