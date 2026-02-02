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

    /// <summary>
    /// Dismiss a DVIR report (quick clear for reports with no issues)
    /// </summary>
    [HttpPost("{id:guid}/dismiss", Name = "DismissDvirReport")]
    [ProducesResponseType(typeof(DvirReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dvir.Review)]
    public async Task<IActionResult> Dismiss(Guid id, [FromBody] DismissDvirReportRequest request)
    {
        var command = new DismissDvirReportCommand
        {
            ReportId = id,
            DismissedById = request.DismissedById,
            Notes = request.Notes
        };
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Reject a DVIR report (sends back to driver for resubmission)
    /// </summary>
    [HttpPost("{id:guid}/reject", Name = "RejectDvirReport")]
    [ProducesResponseType(typeof(DvirReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dvir.Review)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectDvirReportRequest request)
    {
        var command = new RejectDvirReportCommand
        {
            ReportId = id,
            RejectedById = request.RejectedById,
            RejectionReason = request.RejectionReason
        };
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }
}

public record DismissDvirReportRequest(Guid DismissedById, string? Notes);
public record RejectDvirReportRequest(Guid RejectedById, string RejectionReason);
