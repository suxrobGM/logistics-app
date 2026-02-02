using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("safety/accidents")]
[Produces("application/json")]
public class AccidentController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get accident reports with optional filters
    /// </summary>
    [HttpGet(Name = "GetAccidentReports")]
    [ProducesResponseType(typeof(PagedResponse<AccidentReportDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Safety.View)]
    public async Task<IActionResult> GetReports([FromQuery] GetAccidentReportsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<AccidentReportDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    /// <summary>
    /// Get accident report by ID
    /// </summary>
    [HttpGet("{id:guid}", Name = "GetAccidentReportById")]
    [ProducesResponseType(typeof(AccidentReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Safety.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetAccidentReportByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Create a new accident report
    /// </summary>
    [HttpPost(Name = "CreateAccidentReport")]
    [ProducesResponseType(typeof(AccidentReportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Safety.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateAccidentReportCommand request)
    {
        var result = await mediator.Send(request);
        if (!result.IsSuccess)
        {
            return BadRequest(ErrorResponse.FromResult(result));
        }

        return CreatedAtRoute("GetAccidentReportById", new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Update an accident report
    /// </summary>
    [HttpPut("{id:guid}", Name = "UpdateAccidentReport")]
    [ProducesResponseType(typeof(AccidentReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Safety.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccidentReportCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Submit an accident report for review
    /// </summary>
    [HttpPost("{id:guid}/submit", Name = "SubmitAccidentReport")]
    [ProducesResponseType(typeof(AccidentReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Safety.Manage)]
    public async Task<IActionResult> Submit(Guid id)
    {
        var result = await mediator.Send(new SubmitAccidentReportCommand { ReportId = id });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Review an accident report (changes status to UnderReview)
    /// </summary>
    [HttpPost("{id:guid}/review", Name = "ReviewAccidentReport")]
    [ProducesResponseType(typeof(AccidentReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Safety.Manage)]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewAccidentReportRequest request)
    {
        var command = new ReviewAccidentReportCommand
        {
            ReportId = id,
            ReviewedById = request.ReviewedById,
            ReviewNotes = request.ReviewNotes
        };
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Resolve an accident report (changes status to Resolved)
    /// </summary>
    [HttpPost("{id:guid}/resolve", Name = "ResolveAccidentReport")]
    [ProducesResponseType(typeof(AccidentReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Safety.Manage)]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveAccidentReportRequest request)
    {
        var command = new ResolveAccidentReportCommand
        {
            ReportId = id,
            ResolutionNotes = request.ResolutionNotes
        };
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }
}

public record ReviewAccidentReportRequest(Guid ReviewedById, string? ReviewNotes);
public record ResolveAccidentReportRequest(string? ResolutionNotes);
