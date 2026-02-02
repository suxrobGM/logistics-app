using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoadsReportQuery = Logistics.Application.Queries.LoadsReportQuery;
using DriversReportQuery = Logistics.Application.Queries.DriversReportQuery;
using FinancialsReportQuery = Logistics.Application.Queries.FinancialsReportQuery;
using DriverDashboardQuery = Logistics.Application.Queries.DriverDashboardQuery;
using PayrollReportQuery = Logistics.Application.Queries.PayrollReportQuery;
using SafetyReportQuery = Logistics.Application.Queries.SafetyReportQuery;
using MaintenanceReportQuery = Logistics.Application.Queries.MaintenanceReportQuery;

namespace Logistics.API.Controllers;

[ApiController]
[Route("reports")]
[Produces("application/json")]
public class ReportController(IMediator mediator) : ControllerBase
{
    [HttpGet("loads", Name = "GetLoadsReport")]
    [ProducesResponseType(typeof(LoadsReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetLoadsReport([FromQuery] LoadsReportQuery request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }


    [HttpGet("drivers", Name = "GetDriversReport")]
    [ProducesResponseType(typeof(PagedResponse<DriverReportDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetDriversReport([FromQuery] DriversReportQuery request)
    {
        var result = await mediator.Send(request);
        return Ok(PagedResponse<DriverReportDto>.FromPagedResult(result, request.Page, request.PageSize));
    }

    [HttpGet("financials", Name = "GetFinancialsReport")]
    [ProducesResponseType(typeof(FinancialsReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetFinancialsReport([FromQuery] FinancialsReportQuery request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("drivers/dashboard", Name = "GetDriverDashboard")]
    [ProducesResponseType(typeof(DriverDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetDriverDashboard([FromQuery] DriverDashboardQuery request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("payroll", Name = "GetPayrollReport")]
    [ProducesResponseType(typeof(PayrollReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.View)]
    public async Task<IActionResult> GetPayrollReport([FromQuery] PayrollReportQuery request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Get safety report with DVIR, accident, and driver behavior metrics
    /// </summary>
    [HttpGet("safety", Name = "GetSafetyReport")]
    [ProducesResponseType(typeof(SafetyReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Safety.View)]
    public async Task<IActionResult> GetSafetyReport([FromQuery] SafetyReportQuery request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Get maintenance report with service records, costs, and schedule metrics
    /// </summary>
    [HttpGet("maintenance", Name = "GetMaintenanceReport")]
    [ProducesResponseType(typeof(MaintenanceReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Truck.View)]
    public async Task<IActionResult> GetMaintenanceReport([FromQuery] MaintenanceReportQuery request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }
}
