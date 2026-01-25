using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("timeentries")]
[Produces("application/json")]
public class TimeEntryController(IMediator mediator) : ControllerBase
{
    #region Queries

    [HttpGet("{id:guid}", Name = "GetTimeEntryById")]
    [ProducesResponseType(typeof(TimeEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Payroll.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetTimeEntryByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetTimeEntries")]
    [ProducesResponseType(typeof(PagedResponse<TimeEntryDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Payroll.View)]
    public async Task<IActionResult> GetList([FromQuery] GetTimeEntriesQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<TimeEntryDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    #endregion

    #region Commands

    [HttpPost(Name = "CreateTimeEntry")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateTimeEntryCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess
            ? CreatedAtRoute("GetTimeEntryById", new { id = result.Value }, result.Value)
            : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateTimeEntry")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTimeEntryCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteTimeEntry")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteTimeEntryCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    #endregion
}
