using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.Integrations.AIDispatch.Commands;
using Logistics.Application.Modules.Integrations.AIDispatch.Queries;

namespace Logistics.API.Controllers;

[ApiController]
[Route("ai/dispatch")]
[Produces("application/json")]
public class AIDispatchController(IMediator mediator) : ControllerBase
{
    [HttpPost("run", Name = "RunAIDispatch")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> Run([FromBody] RunAIDispatchCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("cancel/{sessionId:guid}", Name = "CancelAIDispatchSession")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> Cancel(Guid sessionId)
    {
        await mediator.Send(new CancelAIDispatchSessionCommand { SessionId = sessionId });
        return NoContent();
    }

    [HttpGet("sessions", Name = "GetAIDispatchSessions")]
    [ProducesResponseType(typeof(PagedResponse<AgentSessionDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Dispatch.View)]
    public async Task<IActionResult> GetSessions([FromQuery] GetAIDispatchSessionsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<AgentSessionDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpGet("sessions/{sessionId:guid}", Name = "GetAIDispatchSessionById")]
    [ProducesResponseType(typeof(AgentSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Dispatch.View)]
    public async Task<IActionResult> GetSession(Guid sessionId)
    {
        var result = await mediator.Send(new GetAIDispatchSessionByIdQuery { SessionId = sessionId });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet("quota", Name = "GetAIQuotaStatus")]
    [ProducesResponseType(typeof(AIQuotaStatusDto), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Dispatch.View)]
    public async Task<IActionResult> GetQuotaStatus()
    {
        var result = await mediator.Send(new GetAIQuotaStatusQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("pending", Name = "GetPendingDecisions")]
    [ProducesResponseType(typeof(List<AgentDecisionDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Dispatch.View)]
    public async Task<IActionResult> GetPendingDecisions()
    {
        var result = await mediator.Send(new GetPendingDecisionsQuery());
        return Ok(result.Value);
    }

    [HttpPost("decisions/{decisionId:guid}/approve", Name = "ApproveAIDispatchDecision")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> ApproveDecision(Guid decisionId)
    {
        var result = await mediator.Send(new ApproveAIDispatchDecisionCommand { DecisionId = decisionId });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("sessions/{sessionId:guid}/replan", Name = "ReplanAIDispatchSession")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> Replan(Guid sessionId, [FromBody] ReplanAIDispatchSessionCommand command)
    {
        command.OriginalSessionId = sessionId;
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("policy", Name = "GetAIDispatchPolicy")]
    [ProducesResponseType(typeof(AIDispatchPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.View)]
    public async Task<IActionResult> GetPolicy()
    {
        var result = await mediator.Send(new GetAIDispatchPolicyQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("policy", Name = "UpdateAIDispatchPolicy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> UpdatePolicy([FromBody] UpdateAIDispatchPolicyCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("policy/regenerate", Name = "RegenerateAIDispatchPolicy")]
    [ProducesResponseType(typeof(AIDispatchPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> RegeneratePolicy()
    {
        var result = await mediator.Send(new RegenerateAIDispatchPolicyCommand());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("policy", Name = "DeleteAIDispatchPolicy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> DeletePolicy()
    {
        var result = await mediator.Send(new DeleteAIDispatchPolicyCommand());
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("decisions/{decisionId:guid}/reject", Name = "RejectAIDispatchDecision")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> RejectDecision(Guid decisionId, [FromBody] RejectAIDispatchDecisionCommand command)
    {
        command.DecisionId = decisionId;
        var result = await mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
