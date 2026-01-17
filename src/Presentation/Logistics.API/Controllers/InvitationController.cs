using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

/// <summary>
///     Controller for managing user invitations.
/// </summary>
[ApiController]
[Route("invitations")]
[Produces("application/json")]
public class InvitationController(IMediator mediator) : ControllerBase
{
    /// <summary>
    ///     Get all invitations for the current tenant.
    /// </summary>
    [HttpGet(Name = "GetInvitations")]
    [ProducesResponseType(typeof(PagedResponse<InvitationDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Invitation.View)]
    public async Task<IActionResult> GetList([FromQuery] GetInvitationsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<InvitationDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    /// <summary>
    ///     Get a specific invitation by ID.
    /// </summary>
    [HttpGet("{id:guid}", Name = "GetInvitationById")]
    [ProducesResponseType(typeof(InvitationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Invitation.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetInvitationByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Create and send a new invitation.
    /// </summary>
    [HttpPost(Name = "CreateInvitation")]
    [ProducesResponseType(typeof(InvitationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invitation.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateInvitationCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Resend an existing invitation email.
    /// </summary>
    [HttpPost("{id:guid}/resend", Name = "ResendInvitation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invitation.Manage)]
    public async Task<IActionResult> Resend(Guid id)
    {
        var result = await mediator.Send(new ResendInvitationCommand { Id = id });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Cancel a pending invitation.
    /// </summary>
    [HttpDelete("{id:guid}", Name = "CancelInvitation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Invitation.Manage)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await mediator.Send(new CancelInvitationCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Validate an invitation token (public endpoint for Identity Server).
    /// </summary>
    [HttpGet("validate/{token}", Name = "ValidateInvitationToken")]
    [ProducesResponseType(typeof(InvitationValidationResult), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateToken(string token)
    {
        var result = await mediator.Send(new ValidateInvitationTokenQuery { Token = token });
        return result.IsSuccess
            ? Ok(result.Value)
            : Ok(new InvitationValidationResult
            {
                IsValid = false,
                ErrorMessage = result.Error
            });
    }

    /// <summary>
    ///     Accept an invitation (public endpoint for Identity Server).
    /// </summary>
    [HttpPost("accept", Name = "AcceptInvitation")]
    [ProducesResponseType(typeof(AcceptInvitationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<IActionResult> Accept([FromBody] AcceptInvitationCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }
}
