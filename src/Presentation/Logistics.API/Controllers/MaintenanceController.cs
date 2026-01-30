using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("maintenance")]
[Produces("application/json")]
public class MaintenanceController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get maintenance records with optional filters
    /// </summary>
    [HttpGet("records", Name = "GetMaintenanceRecords")]
    [ProducesResponseType(typeof(PagedResponse<MaintenanceRecordDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Maintenance.View)]
    public async Task<IActionResult> GetRecords([FromQuery] GetMaintenanceRecordsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<MaintenanceRecordDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    /// <summary>
    /// Get maintenance records for a specific truck
    /// </summary>
    [HttpGet("trucks/{truckId:guid}/history", Name = "GetTruckMaintenanceHistory")]
    [ProducesResponseType(typeof(PagedResponse<MaintenanceRecordDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Maintenance.View)]
    public async Task<IActionResult> GetTruckHistory(Guid truckId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetMaintenanceRecordsQuery { TruckId = truckId, Page = page, PageSize = pageSize };
        var result = await mediator.Send(query);
        return Ok(PagedResponse<MaintenanceRecordDto>.FromPagedResult(result, page, pageSize));
    }

    /// <summary>
    /// Get upcoming maintenance schedules
    /// </summary>
    [HttpGet("upcoming", Name = "GetUpcomingMaintenance")]
    [ProducesResponseType(typeof(List<MaintenanceScheduleDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Maintenance.View)]
    public async Task<IActionResult> GetUpcoming([FromQuery] int daysAhead = 30, [FromQuery] Guid? truckId = null, [FromQuery] bool includeOverdue = true)
    {
        var result = await mediator.Send(new GetUpcomingMaintenanceQuery
        {
            DaysAhead = daysAhead,
            TruckId = truckId,
            IncludeOverdue = includeOverdue
        });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Get a maintenance record by ID
    /// </summary>
    [HttpGet("records/{id:guid}", Name = "GetMaintenanceRecordById")]
    [ProducesResponseType(typeof(MaintenanceRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Maintenance.View)]
    public async Task<IActionResult> GetRecordById(Guid id)
    {
        var result = await mediator.Send(new GetMaintenanceRecordByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Create a new maintenance record
    /// </summary>
    [HttpPost("records", Name = "CreateMaintenanceRecord")]
    [ProducesResponseType(typeof(MaintenanceRecordDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Maintenance.Manage)]
    public async Task<IActionResult> CreateRecord([FromBody] CreateMaintenanceRecordCommand request)
    {
        var result = await mediator.Send(request);
        if (!result.IsSuccess)
        {
            return BadRequest(ErrorResponse.FromResult(result));
        }

        return CreatedAtRoute("GetMaintenanceRecordById", new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Update an existing maintenance record
    /// </summary>
    [HttpPut("records/{id:guid}", Name = "UpdateMaintenanceRecord")]
    [ProducesResponseType(typeof(MaintenanceRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Maintenance.Manage)]
    public async Task<IActionResult> UpdateRecord(Guid id, [FromBody] UpdateMaintenanceRecordCommand request)
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
