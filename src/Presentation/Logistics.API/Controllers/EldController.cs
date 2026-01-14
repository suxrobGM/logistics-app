using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("eld")]
[Produces("application/json")]
public class EldController(IMediator mediator) : ControllerBase
{
    #region Provider Configuration

    [HttpGet("providers", Name = "GetEldProviders")]
    [ProducesResponseType(typeof(List<EldProviderConfigurationDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Eld.View)]
    public async Task<IActionResult> GetProviders()
    {
        var result = await mediator.Send(new GetEldProviderConfigurationsQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("providers", Name = "CreateEldProvider")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Eld.Manage)]
    public async Task<IActionResult> CreateProvider([FromBody] CreateEldProviderConfigurationCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Driver Mappings

    [HttpPost("drivers/mappings", Name = "MapEldDriver")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Eld.Manage)]
    public async Task<IActionResult> MapDriver([FromBody] MapEldDriverCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion

    #region HOS Data

    [HttpGet("drivers/hos", Name = "GetAllDriversHos")]
    [ProducesResponseType(typeof(List<DriverHosStatusDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Eld.View)]
    public async Task<IActionResult> GetAllDriversHos()
    {
        var result = await mediator.Send(new GetAllDriversHosStatusQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("drivers/{employeeId:guid}/logs", Name = "GetDriverHosLogs")]
    [ProducesResponseType(typeof(PagedResponse<HosLogDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Eld.View)]
    public async Task<IActionResult> GetDriverHosLogs(Guid employeeId, [FromQuery] GetDriverHosLogsQuery query)
    {
        query.EmployeeId = employeeId;
        var result = await mediator.Send(query);
        return Ok(PagedResponse<HosLogDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    #endregion

    #region Sync Operations

    [HttpPost("sync/drivers/hos", Name = "SyncAllDriversHos")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Eld.Sync)]
    public async Task<IActionResult> SyncAllDriversHos()
    {
        var result = await mediator.Send(new SyncAllDriversHosStatusCommand());
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion
}
