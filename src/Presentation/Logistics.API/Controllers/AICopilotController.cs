using Logistics.Application.Modules.Integrations.AICopilot.Commands;
using Logistics.Application.Modules.Integrations.AICopilot.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[Route("ai/copilot")]
[ApiController]
[Produces("application/json")]
public class AICopilotController(IMediator mediator) : ControllerBase
{
    [HttpPost("conversations", Name = "CreateCopilotConversation")]
    [ProducesResponseType(typeof(AgentConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Copilot.Manage)]
    public async Task<IActionResult> CreateConversation()
    {
        var result = await mediator.Send(new CreateAICopilotConversationCommand());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("conversations", Name = "GetCopilotConversations")]
    [ProducesResponseType(typeof(PagedResult<AgentConversationDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Copilot.View)]
    public async Task<IActionResult> GetConversations([FromQuery] GetAICopilotConversationsQuery query)
    {
        var result = await mediator.Send(query);
        return result.IsSuccess ? Ok(result) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("conversations/{conversationId:guid}", Name = "GetCopilotConversationById")]
    [ProducesResponseType(typeof(AgentConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Copilot.View)]
    public async Task<IActionResult> GetConversationById(Guid conversationId)
    {
        var result = await mediator.Send(new GetAICopilotConversationByIdQuery { Id = conversationId });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Send a user message. The turn runs in the background; progress and the reply arrive
    ///     over the copilot SignalR hub.
    /// </summary>
    [HttpPost("conversations/{conversationId:guid}/messages", Name = "SendCopilotMessage")]
    [ProducesResponseType(typeof(SendAgentMessageResultDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Copilot.Manage)]
    public async Task<IActionResult> SendMessage(Guid conversationId, [FromBody] SendAICopilotMessageCommand request)
    {
        request.ConversationId = conversationId;
        var result = await mediator.Send(request);
        return result.IsSuccess
            ? Accepted(result.Value)
            : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("quota", Name = "GetCopilotQuotaStatus")]
    [ProducesResponseType(typeof(AIQuotaStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Copilot.View)]
    public async Task<IActionResult> GetQuotaStatus()
    {
        var result = await mediator.Send(new GetAICopilotQuotaStatusQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("conversations/{conversationId:guid}/cancel", Name = "CancelCopilotTurn")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Copilot.Manage)]
    public async Task<IActionResult> CancelTurn(Guid conversationId)
    {
        var result = await mediator.Send(new CancelAICopilotTurnCommand { ConversationId = conversationId });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("conversations/{conversationId:guid}", Name = "RenameCopilotConversation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Copilot.Manage)]
    public async Task<IActionResult> RenameConversation(
        Guid conversationId, [FromBody] RenameAICopilotConversationCommand request)
    {
        request.ConversationId = conversationId;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("conversations/{conversationId:guid}", Name = "DeleteCopilotConversation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Copilot.Manage)]
    public async Task<IActionResult> DeleteConversation(Guid conversationId)
    {
        var result = await mediator.Send(new DeleteAICopilotConversationCommand { ConversationId = conversationId });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("decisions/{decisionId:guid}/approve", Name = "ApproveCopilotDecision")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Copilot.Manage)]
    public async Task<IActionResult> ApproveDecision(Guid decisionId)
    {
        var result = await mediator.Send(new ApproveAICopilotDecisionCommand { DecisionId = decisionId });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("decisions/{decisionId:guid}/reject", Name = "RejectCopilotDecision")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Copilot.Manage)]
    public async Task<IActionResult> RejectDecision(Guid decisionId, [FromBody] RejectAICopilotDecisionCommand request)
    {
        request.DecisionId = decisionId;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
