using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("safety/emergency")]
[Produces("application/json")]
public class EmergencyController(IMediator mediator) : ControllerBase
{
    #region Emergency Alerts

    /// <summary>
    /// Get all emergency alerts with optional filters
    /// </summary>
    [HttpGet("alerts", Name = "GetEmergencyAlerts")]
    [ProducesResponseType(typeof(PagedResponse<EmergencyAlertDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Safety.View)]
    public async Task<IActionResult> GetAlerts([FromQuery] GetEmergencyAlertsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<EmergencyAlertDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    /// <summary>
    /// Get active emergency alerts (not resolved)
    /// </summary>
    [HttpGet("alerts/active", Name = "GetActiveEmergencyAlerts")]
    [ProducesResponseType(typeof(PagedResponse<EmergencyAlertDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Safety.View)]
    public async Task<IActionResult> GetActiveAlerts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetActiveEmergencyAlertsQuery { Page = page, PageSize = pageSize };
        var result = await mediator.Send(query);
        return Ok(PagedResponse<EmergencyAlertDto>.FromPagedResult(result, page, pageSize));
    }

    /// <summary>
    /// Get emergency alert by ID
    /// </summary>
    [HttpGet("alerts/{id:guid}", Name = "GetEmergencyAlertById")]
    [ProducesResponseType(typeof(EmergencyAlertDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Safety.View)]
    public async Task<IActionResult> GetAlertById(Guid id)
    {
        var result = await mediator.Send(new GetEmergencyAlertByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Trigger a new emergency alert
    /// </summary>
    [HttpPost("trigger", Name = "TriggerEmergencyAlert")]
    [ProducesResponseType(typeof(EmergencyAlertDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Safety.Manage)]
    public async Task<IActionResult> Trigger([FromBody] TriggerEmergencyAlertCommand request)
    {
        var result = await mediator.Send(request);
        if (!result.IsSuccess)
        {
            return BadRequest(ErrorResponse.FromResult(result));
        }

        return CreatedAtRoute("GetEmergencyAlertById", new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Acknowledge an emergency alert
    /// </summary>
    [HttpPost("alerts/{id:guid}/acknowledge", Name = "AcknowledgeEmergencyAlert")]
    [ProducesResponseType(typeof(EmergencyAlertDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Safety.Manage)]
    public async Task<IActionResult> Acknowledge(Guid id, [FromBody] AcknowledgeEmergencyAlertRequest request)
    {
        var result = await mediator.Send(new AcknowledgeEmergencyAlertCommand(id, request.AcknowledgedById, request.DispatcherNotes));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Resolve an emergency alert
    /// </summary>
    [HttpPost("alerts/{id:guid}/resolve", Name = "ResolveEmergencyAlert")]
    [ProducesResponseType(typeof(EmergencyAlertDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Safety.Manage)]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveEmergencyAlertRequest request)
    {
        var result = await mediator.Send(new ResolveEmergencyAlertCommand(id, request.ResolvedById, request.ResolutionNotes, request.IsFalseAlarm));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Emergency Contacts

    /// <summary>
    /// Get emergency contacts with optional filters
    /// </summary>
    [HttpGet("contacts", Name = "GetEmergencyContacts")]
    [ProducesResponseType(typeof(PagedResponse<EmergencyContactDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Safety.View)]
    public async Task<IActionResult> GetContacts([FromQuery] GetEmergencyContactsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<EmergencyContactDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    /// <summary>
    /// Create a new emergency contact
    /// </summary>
    [HttpPost("contacts", Name = "CreateEmergencyContact")]
    [ProducesResponseType(typeof(EmergencyContactDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Safety.Manage)]
    public async Task<IActionResult> CreateContact([FromBody] CreateEmergencyContactCommand request)
    {
        var result = await mediator.Send(request);
        if (!result.IsSuccess)
        {
            return BadRequest(ErrorResponse.FromResult(result));
        }

        return CreatedAtRoute("GetEmergencyContacts", null, result.Value);
    }

    /// <summary>
    /// Update an emergency contact
    /// </summary>
    [HttpPut("contacts/{id:guid}", Name = "UpdateEmergencyContact")]
    [ProducesResponseType(typeof(EmergencyContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Safety.Manage)]
    public async Task<IActionResult> UpdateContact(Guid id, [FromBody] UpdateEmergencyContactCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Delete an emergency contact
    /// </summary>
    [HttpDelete("contacts/{id:guid}", Name = "DeleteEmergencyContact")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Safety.Manage)]
    public async Task<IActionResult> DeleteContact(Guid id)
    {
        var result = await mediator.Send(new DeleteEmergencyContactCommand(id));
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion
}
