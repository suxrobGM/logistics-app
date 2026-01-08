using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LoadsReportQuery = Logistics.Application.Queries.LoadsReportQuery;
using DriversReportQuery = Logistics.Application.Queries.DriversReportQuery;
using FinancialsReportQuery = Logistics.Application.Queries.FinancialsReportQuery;
using DriverDashboardQuery = Logistics.Application.Queries.DriverDashboardQuery;

namespace Logistics.API.Controllers;

[ApiController]
[Route("reports")]
public class ReportController(IMediator mediator) : ControllerBase
{
    [HttpGet("loads", Name = "GetLoadsReport")]
    [ProducesResponseType(typeof(LoadsReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetLoadsReport([FromQuery] LoadsReportQuery request)
    {
        var result = await mediator.Send(request);
        return result.Success ? Ok(result.Data) : BadRequest(ErrorResponse.FromResult(result));
    }


    [HttpGet("drivers", Name = "GetDriversReport")]
    [ProducesResponseType(typeof(PagedResponse<DriverReportDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetDriversReport([FromQuery] DriversReportQuery request)
    {
        var result = await mediator.Send(request);
        return Ok(PagedResponse<DriverReportDto>.FromPagedResult(result, request.Page, request.PageSize));
    }

    [HttpGet("financials", Name = "GetFinancialsReport")]
    [ProducesResponseType(typeof(FinancialsReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetFinancialsReport([FromQuery] FinancialsReportQuery request)
    {
        var result = await mediator.Send(request);
        return result.Success ? Ok(result.Data) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("drivers/dashboard", Name = "GetDriverDashboard")]
    [ProducesResponseType(typeof(DriverDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetDriverDashboard([FromQuery] DriverDashboardQuery request)
    {
        var result = await mediator.Send(request);
        return result.Success ? Ok(result.Data) : BadRequest(ErrorResponse.FromResult(result));
    }
}
