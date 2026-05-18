using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.IdentityAccess.Tenants.Commands;
using Logistics.Application.Modules.Integrations.AiDispatch.Commands;
using Logistics.Application.Modules.Integrations.AiDispatch.Queries;

namespace Logistics.API.Controllers;

[ApiController]
[Route("ai/dispatch")]
[Produces("application/json")]
public class AiDispatchController(IMediator mediator) : ControllerBase
{
    [HttpPost("run", Name = "RunAiDispatch")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> Run([FromBody] RunAiDispatchCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("cancel/{sessionId:guid}", Name = "CancelAiDispatchSession")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> Cancel(Guid sessionId)
    {
        await mediator.Send(new CancelAiDispatchSessionCommand { SessionId = sessionId });
        return NoContent();
    }

    [HttpGet("sessions", Name = "GetAiDispatchSessions")]
    [ProducesResponseType(typeof(PagedResponse<AiDispatchSessionDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Dispatch.View)]
    public async Task<IActionResult> GetSessions([FromQuery] GetAiDispatchSessionsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<AiDispatchSessionDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpGet("sessions/{sessionId:guid}", Name = "GetAiDispatchSessionById")]
    [ProducesResponseType(typeof(AiDispatchSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Dispatch.View)]
    public async Task<IActionResult> GetSession(Guid sessionId)
    {
        var result = await mediator.Send(new GetAiDispatchSessionByIdQuery { SessionId = sessionId });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet("quota", Name = "GetAiQuotaStatus")]
    [ProducesResponseType(typeof(AiQuotaStatusDto), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Dispatch.View)]
    public async Task<IActionResult> GetQuotaStatus()
    {
        var result = await mediator.Send(new GetAiQuotaStatusQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("pending", Name = "GetPendingDecisions")]
    [ProducesResponseType(typeof(List<AiDispatchDecisionDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Dispatch.View)]
    public async Task<IActionResult> GetPendingDecisions()
    {
        var result = await mediator.Send(new GetPendingDecisionsQuery());
        return Ok(result.Value);
    }

    [HttpPost("decisions/{decisionId:guid}/approve", Name = "ApproveAiDispatchDecision")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> ApproveDecision(Guid decisionId)
    {
        var result = await mediator.Send(new ApproveAiDispatchDecisionCommand { DecisionId = decisionId });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("sessions/{sessionId:guid}/replan", Name = "ReplanAiDispatchSession")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> Replan(Guid sessionId, [FromBody] ReplanAiDispatchSessionCommand command)
    {
        command.OriginalSessionId = sessionId;
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("settings", Name = "UpdateTenantAiSettings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> UpdateAiSettings([FromBody] UpdateTenantAiSettingsCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("decisions/{decisionId:guid}/reject", Name = "RejectAiDispatchDecision")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> RejectDecision(Guid decisionId, [FromBody] RejectAiDispatchDecisionCommand command)
    {
        command.DecisionId = decisionId;
        var result = await mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
