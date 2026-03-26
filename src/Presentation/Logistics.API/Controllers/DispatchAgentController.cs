using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("dispatch/agent")]
[Produces("application/json")]
public class DispatchAgentController(IMediator mediator) : ControllerBase
{
    [HttpPost("run", Name = "RunDispatchAgent")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> Run([FromBody] RunDispatchAgentCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("cancel/{sessionId:guid}", Name = "CancelDispatchSession")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> Cancel(Guid sessionId)
    {
        await mediator.Send(new CancelDispatchSessionCommand { SessionId = sessionId });
        return NoContent();
    }

    [HttpGet("sessions", Name = "GetDispatchSessions")]
    [ProducesResponseType(typeof(PagedResponse<DispatchSessionDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Dispatch.View)]
    public async Task<IActionResult> GetSessions([FromQuery] GetDispatchSessionsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<DispatchSessionDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpGet("sessions/{sessionId:guid}", Name = "GetDispatchSessionById")]
    [ProducesResponseType(typeof(DispatchSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Dispatch.View)]
    public async Task<IActionResult> GetSession(Guid sessionId)
    {
        var result = await mediator.Send(new GetDispatchSessionByIdQuery { SessionId = sessionId });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet("pending", Name = "GetPendingDecisions")]
    [ProducesResponseType(typeof(List<DispatchDecisionDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Dispatch.View)]
    public async Task<IActionResult> GetPendingDecisions()
    {
        var result = await mediator.Send(new GetPendingDecisionsQuery());
        return Ok(result.Value);
    }

    [HttpPost("decisions/{decisionId:guid}/approve", Name = "ApproveDispatchDecision")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> ApproveDecision(Guid decisionId)
    {
        var result = await mediator.Send(new ApproveDispatchDecisionCommand { DecisionId = decisionId });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("decisions/{decisionId:guid}/reject", Name = "RejectDispatchDecision")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Dispatch.Manage)]
    public async Task<IActionResult> RejectDecision(Guid decisionId, [FromBody] RejectDispatchDecisionCommand command)
    {
        command.DecisionId = decisionId;
        var result = await mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
