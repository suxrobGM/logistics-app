using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CreateTruckCommand = Logistics.Application.Commands.CreateTruckCommand;
using GetTruckQuery = Logistics.Application.Queries.GetTruckQuery;
using UpdateTruckCommand = Logistics.Application.Commands.UpdateTruckCommand;

namespace Logistics.API.Controllers;

[ApiController]
[Route("trucks")]
public class TruckController(IMediator mediator) : ControllerBase
{
    [HttpGet("{truckOrDriverId:guid}", Name = "GetTruckById")]
    [ProducesResponseType(typeof(TruckDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permissions.Trucks.View)]
    public async Task<IActionResult> GetById(Guid truckOrDriverId, [FromQuery] GetTruckQuery request)
    {
        request.TruckOrDriverId = truckOrDriverId;
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetTrucks")]
    [ProducesResponseType(typeof(PagedResponse<TruckDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permissions.Trucks.View)]
    public async Task<IActionResult> GetList([FromQuery] GetTrucksQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<TruckDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpPost(Name = "CreateTruck")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Trucks.Create)]
    public async Task<IActionResult> Create([FromBody] CreateTruckCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateTruck")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Trucks.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTruckCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteTruck")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permissions.Trucks.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteTruckCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }
}
