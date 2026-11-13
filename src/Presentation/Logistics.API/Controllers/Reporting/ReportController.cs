using Logistics.API.Extensions;
using Logistics.Application.Modules.Reporting.Reports.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers.Reporting;

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
        return result.ToActionResult();
    }


    [HttpGet("drivers", Name = "GetDriversReport")]
    [ProducesResponseType(typeof(PagedResponse<DriverReportDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetDriversReport([FromQuery] DriversReportQuery request)
    {
        var result = await mediator.Send(request);
        return Ok(PagedResponse<DriverReportDto>.FromPagedResult(result, request.Page, request.PageSize));
    }

    [HttpGet("ifta", Name = "GetIftaReport")]
    [ProducesResponseType(typeof(IftaReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetIftaReport([FromQuery] IftaReportQuery request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpGet("ifta/pdf", Name = "ExportIftaReportPdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> ExportIftaReportPdf([FromQuery] ExportIftaReportPdfQuery request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess
            ? File(result.Value!, "application/pdf", $"ifta-report-{request.Year}-q{request.Quarter}.pdf")
            : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("financials", Name = "GetFinancialsReport")]
    [ProducesResponseType(typeof(FinancialsReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetFinancialsReport([FromQuery] FinancialsReportQuery request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpGet("profitability/loads", Name = "GetLoadsProfitability")]
    [ProducesResponseType(typeof(PagedResponse<LoadProfitDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetLoadsProfitability([FromQuery] LoadsProfitabilityQuery request)
    {
        var result = await mediator.Send(request);
        return Ok(PagedResponse<LoadProfitDto>.FromPagedResult(result, request.Page, request.PageSize));
    }

    [HttpGet("profitability/trucks", Name = "GetTrucksProfitability")]
    [ProducesResponseType(typeof(List<TruckProfitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetTrucksProfitability([FromQuery] TrucksProfitabilityQuery request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpGet("profitability/lanes", Name = "GetLanesProfitability")]
    [ProducesResponseType(typeof(List<LaneProfitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetLanesProfitability([FromQuery] LanesProfitabilityQuery request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpGet("profitability/summary", Name = "GetProfitabilitySummary")]
    [ProducesResponseType(typeof(ProfitabilitySummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetProfitabilitySummary([FromQuery] ProfitabilitySummaryQuery request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpGet("drivers/dashboard", Name = "GetDriverDashboard")]
    [ProducesResponseType(typeof(DriverDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetDriverDashboard([FromQuery] DriverDashboardQuery request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpGet("payroll", Name = "GetPayrollReport")]
    [ProducesResponseType(typeof(PayrollReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.View)]
    public async Task<IActionResult> GetPayrollReport([FromQuery] PayrollReportQuery request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get safety report with DVIR and accident metrics
    /// </summary>
    [HttpGet("safety", Name = "GetSafetyReport")]
    [ProducesResponseType(typeof(SafetyReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Safety.View)]
    public async Task<IActionResult> GetSafetyReport([FromQuery] SafetyReportQuery request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
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
        return result.ToActionResult();
    }
}
