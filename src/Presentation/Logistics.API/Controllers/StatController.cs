using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using GetDailyGrossesQuery = Logistics.Application.Queries.GetDailyGrossesQuery;
using GetMonthlyGrossesQuery = Logistics.Application.Queries.GetMonthlyGrossesQuery;

namespace Logistics.API.Controllers;

[ApiController]
[Route("stats")]
public class StatController(IMediator mediator) : ControllerBase
{
    [HttpGet("daily-grosses", Name = "GetDailyGrosses")]
    [ProducesResponseType(typeof(Result<DailyGrossesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetDailyGrosses([FromQuery] GetDailyGrossesQuery request)
    {
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("monthly-grosses", Name = "GetMonthlyGrosses")]
    [ProducesResponseType(typeof(Result<MonthlyGrossesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetMonthlyGrosses([FromQuery] GetMonthlyGrossesQuery request)
    {
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("company", Name = "GetCompanyStats")]
    [ProducesResponseType(typeof(Result<CompanyStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetCompanyStats([FromQuery] GetCompanyStatsQuery request)
    {
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("trucks", Name = "GetTrucksStats")]
    [ProducesResponseType(typeof(PagedResult<TruckStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetTrucksStatsList([FromQuery] GetTrucksStatsListQuery request)
    {
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("driver/{userId:guid}", Name = "GetDriverStats")]
    [ProducesResponseType(typeof(Result<DriverStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetDriverStats(Guid userId, [FromQuery] GetDriverStatsQuery request)
    {
        request.UserId = userId;
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
