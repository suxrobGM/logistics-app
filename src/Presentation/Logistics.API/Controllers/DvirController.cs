using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("dvir")]
[Produces("application/json")]
public class DvirController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get DVIR reports with optional filters
    /// </summary>
    [HttpGet(Name = "GetDvirReports")]
    [ProducesResponseType(typeof(PagedResponse<DvirReportDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Dvir.View)]
    public async Task<IActionResult> GetReports([FromQuery] GetDvirReportsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<DvirReportDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    /// <summary>
    /// Get DVIR report by ID
    /// </summary>
    [HttpGet("{id:guid}", Name = "GetDvirReportById")]
    [ProducesResponseType(typeof(DvirReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Dvir.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetDvirReportByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Get DVIR reports pending mechanic review
    /// </summary>
    [HttpGet("pending-reviews", Name = "GetPendingDvirReviews")]
    [ProducesResponseType(typeof(List<DvirReportDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Dvir.Review)]
    public async Task<IActionResult> GetPendingReviews()
    {
        var result = await mediator.Send(new GetPendingDvirReviewsQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Create a new DVIR report
    /// </summary>
    [HttpPost(Name = "CreateDvirReport")]
    [ProducesResponseType(typeof(DvirReportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dvir.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateDvirReportCommand request)
    {
        var result = await mediator.Send(request);
        if (!result.IsSuccess)
        {
            return BadRequest(ErrorResponse.FromResult(result));
        }

        return CreatedAtRoute("GetDvirReportById", new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Submit a DVIR report for review
    /// </summary>
    [HttpPost("{id:guid}/submit", Name = "SubmitDvirReport")]
    [ProducesResponseType(typeof(DvirReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dvir.Manage)]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitDvirReportCommand request)
    {
        request.ReportId = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Mechanic review and sign-off on a DVIR report
    /// </summary>
    [HttpPost("{id:guid}/review", Name = "ReviewDvirReport")]
    [ProducesResponseType(typeof(DvirReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dvir.Review)]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewDvirReportCommand request)
    {
        request.ReportId = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }
}
