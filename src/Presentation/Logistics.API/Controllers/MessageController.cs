using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Messaging;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("messages")]
[Produces("application/json")]
public class MessageController(IMediator mediator) : ControllerBase
{
    #region Conversations

    [HttpGet("conversations", Name = "GetConversations")]
    [ProducesResponseType(typeof(ConversationDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Message.View)]
    public async Task<IActionResult> GetConversations([FromQuery] GetConversationsQuery request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("conversations", Name = "CreateConversation")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Message.Manage)]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var command = new CreateConversationCommand
        {
            Name = request.Name,
            LoadId = request.LoadId,
            ParticipantIds = request.ParticipantIds
        };
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetConversations), new { }, result.Value)
            : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("conversations/{conversationId:guid}", Name = "GetMessages")]
    [ProducesResponseType(typeof(MessageDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Message.View)]
    public async Task<IActionResult> GetMessages(
        Guid conversationId,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        [FromQuery] DateTime? before = null)
    {
        var query = new GetMessagesQuery
        {
            ConversationId = conversationId,
            Limit = limit,
            Offset = offset,
            Before = before
        };
        var result = await mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Messages

    [HttpPost(Name = "SendMessage")]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Message.Manage)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        // Get sender ID from the current user claims
        var senderIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(senderIdClaim) || !Guid.TryParse(senderIdClaim, out var senderId))
        {
            return BadRequest(new ErrorResponse("Unable to identify sender"));
        }

        var command = new SendMessageCommand
        {
            ConversationId = request.ConversationId,
            SenderId = senderId,
            Content = request.Content
        };
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetMessages), new { conversationId = request.ConversationId }, result.Value)
            : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{messageId:guid}/read", Name = "MarkMessageRead")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Message.View)]
    public async Task<IActionResult> MarkAsRead(Guid messageId)
    {
        // Get reader ID from the current user claims
        var readerIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(readerIdClaim) || !Guid.TryParse(readerIdClaim, out var readerId))
        {
            return BadRequest(new ErrorResponse("Unable to identify reader"));
        }

        var command = new MarkMessageReadCommand
        {
            MessageId = messageId,
            ReadById = readerId
        };
        var result = await mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Unread Count

    [HttpGet("unread-count", Name = "GetUnreadCount")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Message.View)]
    public async Task<IActionResult> GetUnreadCount()
    {
        // Get employee ID from the current user claims
        var employeeIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(employeeIdClaim) || !Guid.TryParse(employeeIdClaim, out var employeeId))
        {
            return BadRequest(new ErrorResponse("Unable to identify user"));
        }

        var query = new GetUnreadCountQuery { EmployeeId = employeeId };
        var result = await mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion
}
