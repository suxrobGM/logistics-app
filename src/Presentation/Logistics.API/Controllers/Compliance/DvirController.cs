using Logistics.API.Extensions;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Logistics.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.Compliance.Dvir.Commands;
using Logistics.Application.Modules.Compliance.Dvir.Queries;

namespace Logistics.API.Controllers.Compliance;

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
        return result.ToActionResult();
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
        return result.ToActionResult();
    }

    /// <summary>
    /// Get uncorrected defects from the truck's latest pre-trip and post-trip reports
    /// </summary>
    [HttpGet("trucks/{truckId:guid}/open-defects", Name = "GetTruckOpenDvirDefects")]
    [ProducesResponseType(typeof(List<DvirDefectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dvir.View)]
    public async Task<IActionResult> GetTruckOpenDefects(Guid truckId)
    {
        var result = await mediator.Send(new GetTruckOpenDvirDefectsQuery(truckId));
        return result.ToActionResult();
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
        return result.ToCreatedResult("GetDvirReportById", x => new { id = x.Id });
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
        return result.ToActionResult();
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
        return result.ToActionResult();
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
            Notes = request.Notes
        };
        var result = await mediator.Send(command);
        return result.ToActionResult();
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
            RejectionReason = request.RejectionReason
        };
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
}

public record DismissDvirReportRequest(string? Notes);
public record RejectDvirReportRequest(string RejectionReason);
