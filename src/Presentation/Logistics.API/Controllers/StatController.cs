using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using GetDailyGrossesQuery = Logistics.Application.Queries.GetDailyGrossesQuery;
using GetMonthlyGrossesQuery = Logistics.Application.Queries.GetMonthlyGrossesQuery;

namespace Logistics.API.Controllers;

[Route("stats")]
[ApiController]
public class StatController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("daily-grosses")]
    [ProducesResponseType(typeof(Result<DailyGrossesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetDailyGrosses([FromQuery] GetDailyGrossesQuery request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("monthly-grosses")]
    [ProducesResponseType(typeof(Result<MonthlyGrossesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetMonthlyGrosses([FromQuery] GetMonthlyGrossesQuery request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("company")]
    [ProducesResponseType(typeof(Result<CompanyStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetCompanyStats([FromQuery] GetCompanyStatsQuery request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("trucks")]
    [ProducesResponseType(typeof(PagedResult<TruckStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetTrucksStatsList([FromQuery] GetTrucksStatsListQuery request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("driver/{userId:guid}")]
    [ProducesResponseType(typeof(PagedResult<DriverStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetDriverStatsList(Guid userId, [FromQuery] GetDriverStatsQuery request)
    {
        request.UserId = userId;
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
