using Logistics.API.Extensions;
using Logistics.Application.Modules.AI.AIDispatch.Commands;
using Logistics.Application.Modules.AI.AIDispatch.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers.AI;

[ApiController]
[Route("ai/dispatch")]
[Produces("application/json")]
public class AIDispatchController(IMediator mediator) : ControllerBase
{
    [HttpPost("conversations", Name = "CreateAIDispatchConversation")]
    [ProducesResponseType(typeof(AgentConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AIDispatch.Manage)]
    public async Task<IActionResult> CreateConversation()
    {
        var result = await mediator.Send(new CreateAIDispatchConversationCommand());
        return result.ToActionResult();
    }

    [HttpGet("conversations", Name = "GetAIDispatchConversations")]
    [ProducesResponseType(typeof(PagedResponse<AgentConversationDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.AIDispatch.View)]
    public async Task<IActionResult> GetConversations([FromQuery] GetAIDispatchConversationsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<AgentConversationDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpGet("conversations/{conversationId:guid}", Name = "GetAIDispatchConversationById")]
    [ProducesResponseType(typeof(AgentConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.AIDispatch.View)]
    public async Task<IActionResult> GetConversationById(Guid conversationId)
    {
        var result = await mediator.Send(new GetAIDispatchConversationByIdQuery { Id = conversationId });
        return result.ToActionResult();
    }

    /// <summary>
    ///     Send a message to a tenant-shared dispatch conversation. The turn runs in the background;
    ///     progress and the reply arrive over the dispatch SignalR hub for every connected client.
    /// </summary>
    [HttpPost("conversations/{conversationId:guid}/messages", Name = "SendAIDispatchMessage")]
    [ProducesResponseType(typeof(SendAgentMessageResultDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AIDispatch.Manage)]
    public async Task<IActionResult> SendMessage(Guid conversationId, [FromBody] SendAIDispatchMessageCommand request)
    {
        request.ConversationId = conversationId;
        var result = await mediator.Send(request);
        return result.IsSuccess
            ? Accepted(result.Value)
            : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("conversations/{conversationId:guid}/cancel", Name = "CancelAIDispatchTurn")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AIDispatch.Manage)]
    public async Task<IActionResult> CancelTurn(Guid conversationId)
    {
        var result = await mediator.Send(new CancelAIDispatchTurnCommand { ConversationId = conversationId });
        return result.ToActionResult();
    }

    [HttpPut("conversations/{conversationId:guid}", Name = "RenameAIDispatchConversation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AIDispatch.Manage)]
    public async Task<IActionResult> RenameConversation(
        Guid conversationId, [FromBody] RenameAIDispatchConversationCommand request)
    {
        request.ConversationId = conversationId;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpDelete("conversations/{conversationId:guid}", Name = "DeleteAIDispatchConversation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AIDispatch.Manage)]
    public async Task<IActionResult> DeleteConversation(Guid conversationId)
    {
        var result = await mediator.Send(new DeleteAIDispatchConversationCommand { ConversationId = conversationId });
        return result.ToActionResult();
    }

    [HttpGet("quota", Name = "GetAIQuotaStatus")]
    [ProducesResponseType(typeof(AIQuotaStatusDto), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.AIDispatch.View)]
    public async Task<IActionResult> GetQuotaStatus()
    {
        var result = await mediator.Send(new GetAIQuotaStatusQuery());
        return result.ToActionResult();
    }

    [HttpGet("pending", Name = "GetPendingDecisions")]
    [ProducesResponseType(typeof(List<AgentDecisionDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.AIDispatch.View)]
    public async Task<IActionResult> GetPendingDecisions()
    {
        var result = await mediator.Send(new GetPendingDecisionsQuery());
        return Ok(result.Value);
    }

    [HttpPost("decisions/{decisionId:guid}/approve", Name = "ApproveAIDispatchDecision")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AIDispatch.Manage)]
    public async Task<IActionResult> ApproveDecision(Guid decisionId)
    {
        var result = await mediator.Send(new ApproveAIDispatchDecisionCommand { DecisionId = decisionId });
        return result.ToActionResult();
    }

    [HttpPost("decisions/{decisionId:guid}/reject", Name = "RejectAIDispatchDecision")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AIDispatch.Manage)]
    public async Task<IActionResult> RejectDecision(Guid decisionId, [FromBody] RejectAIDispatchDecisionCommand command)
    {
        command.DecisionId = decisionId;
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>Allowed for 15 minutes after an auto-approved decision runs.</summary>
    [HttpPost("decisions/{decisionId:guid}/revert", Name = "RevertAIDispatchDecision")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AIDispatch.Manage)]
    public async Task<IActionResult> RevertDecision(Guid decisionId)
    {
        var result = await mediator.Send(new RevertAIDispatchDecisionCommand { DecisionId = decisionId });
        return result.ToActionResult();
    }

    [HttpGet("autonomy", Name = "GetAIDispatchAutonomy")]
    [ProducesResponseType(typeof(AIDispatchAutonomyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AIDispatch.View)]
    public async Task<IActionResult> GetAutonomy()
    {
        var result = await mediator.Send(new GetAIDispatchAutonomyQuery());
        return result.ToActionResult();
    }

    [HttpPut("autonomy", Name = "UpdateAIDispatchAutonomy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AIDispatch.Manage)]
    public async Task<IActionResult> UpdateAutonomy([FromBody] UpdateAIDispatchAutonomyCommand command)
    {
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpGet("policy", Name = "GetAIDispatchPolicy")]
    [ProducesResponseType(typeof(AIDispatchPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AIDispatch.View)]
    public async Task<IActionResult> GetPolicy()
    {
        var result = await mediator.Send(new GetAIDispatchPolicyQuery());
        return result.ToActionResult();
    }

    [HttpPut("policy", Name = "UpdateAIDispatchPolicy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AIDispatch.Manage)]
    public async Task<IActionResult> UpdatePolicy([FromBody] UpdateAIDispatchPolicyCommand command)
    {
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("policy/regenerate", Name = "RegenerateAIDispatchPolicy")]
    [ProducesResponseType(typeof(AIDispatchPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AIDispatch.Manage)]
    public async Task<IActionResult> RegeneratePolicy()
    {
        var result = await mediator.Send(new RegenerateAIDispatchPolicyCommand());
        return result.ToActionResult();
    }

    [HttpDelete("policy", Name = "DeleteAIDispatchPolicy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AIDispatch.Manage)]
    public async Task<IActionResult> DeletePolicy()
    {
        var result = await mediator.Send(new DeleteAIDispatchPolicyCommand());
        return result.ToActionResult();
    }
}
