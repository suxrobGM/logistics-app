using Logistics.API.Extensions;
using Logistics.Application.Modules.AI.AICopilot.Commands;
using Logistics.Application.Modules.AI.AICopilot.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers.AI;

[Route("ai/copilot")]
[ApiController]
[Produces("application/json")]
public class AICopilotController(IMediator mediator) : ControllerBase
{
    [HttpPost("conversations", Name = "CreateCopilotConversation")]
    [ProducesResponseType(typeof(AgentConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AICopilot.Manage)]
    public async Task<IActionResult> CreateConversation()
    {
        var result = await mediator.Send(new CreateAICopilotConversationCommand());
        return result.ToActionResult();
    }

    [HttpGet("conversations", Name = "GetCopilotConversations")]
    [ProducesResponseType(typeof(PagedResponse<AgentConversationDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.AICopilot.View)]
    public async Task<IActionResult> GetConversations([FromQuery] GetAICopilotConversationsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<AgentConversationDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpGet("conversations/{conversationId:guid}", Name = "GetCopilotConversationById")]
    [ProducesResponseType(typeof(AgentConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AICopilot.View)]
    public async Task<IActionResult> GetConversationById(Guid conversationId)
    {
        var result = await mediator.Send(new GetAICopilotConversationByIdQuery { Id = conversationId });
        return result.ToActionResult();
    }

    /// <summary>Posts a user message and starts a copilot turn in the background (202 Accepted); the reply is pushed over the copilot SignalR hub.</summary>
    [HttpPost("conversations/{conversationId:guid}/messages", Name = "SendCopilotMessage")]
    [ProducesResponseType(typeof(SendAgentMessageResultDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AICopilot.Manage)]
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
    [Authorize(Policy = Permission.AICopilot.View)]
    public async Task<IActionResult> GetQuotaStatus()
    {
        var result = await mediator.Send(new GetAICopilotQuotaStatusQuery());
        return result.ToActionResult();
    }

    [HttpPost("conversations/{conversationId:guid}/cancel", Name = "CancelCopilotTurn")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AICopilot.Manage)]
    public async Task<IActionResult> CancelTurn(Guid conversationId)
    {
        var result = await mediator.Send(new CancelAICopilotTurnCommand { ConversationId = conversationId });
        return result.ToActionResult();
    }

    [HttpPut("conversations/{conversationId:guid}", Name = "RenameCopilotConversation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AICopilot.Manage)]
    public async Task<IActionResult> RenameConversation(
        Guid conversationId, [FromBody] RenameAICopilotConversationCommand request)
    {
        request.ConversationId = conversationId;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpDelete("conversations/{conversationId:guid}", Name = "DeleteCopilotConversation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AICopilot.Manage)]
    public async Task<IActionResult> DeleteConversation(Guid conversationId)
    {
        var result = await mediator.Send(new DeleteAICopilotConversationCommand { ConversationId = conversationId });
        return result.ToActionResult();
    }

    [HttpPost("decisions/{decisionId:guid}/approve", Name = "ApproveCopilotDecision")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AICopilot.Manage)]
    public async Task<IActionResult> ApproveDecision(Guid decisionId)
    {
        var result = await mediator.Send(new ApproveAICopilotDecisionCommand { DecisionId = decisionId });
        return result.ToActionResult();
    }

    [HttpPost("decisions/{decisionId:guid}/reject", Name = "RejectCopilotDecision")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.AICopilot.Manage)]
    public async Task<IActionResult> RejectDecision(Guid decisionId, [FromBody] RejectAICopilotDecisionCommand request)
    {
        request.DecisionId = decisionId;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }
}
