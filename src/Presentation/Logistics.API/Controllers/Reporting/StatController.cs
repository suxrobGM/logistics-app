using Logistics.API.Extensions;
using Logistics.Application.Modules.Reporting.Stats.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers.Reporting;

[ApiController]
[Route("stats")]
[Produces("application/json")]
public class StatController(IMediator mediator) : ControllerBase
{
    [HttpGet("daily-grosses", Name = "GetDailyGrosses")]
    [ProducesResponseType(typeof(DailyGrossesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetDailyGrosses([FromQuery] GetDailyGrossesQuery request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpGet("monthly-grosses", Name = "GetMonthlyGrosses")]
    [ProducesResponseType(typeof(MonthlyGrossesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetMonthlyGrosses([FromQuery] GetMonthlyGrossesQuery request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpGet("company", Name = "GetCompanyStats")]
    [ProducesResponseType(typeof(CompanyStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetCompanyStats([FromQuery] GetCompanyStatsQuery request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpGet("driver/{userId:guid}", Name = "GetDriverStats")]
    [ProducesResponseType(typeof(DriverStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetDriverStats(Guid userId, [FromQuery] GetDriverStatsQuery request)
    {
        request.UserId = userId;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }
}
