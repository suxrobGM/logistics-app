using Logistics.Application.Modules.IdentityAccess.Admins.Commands;
using Logistics.Application.Modules.IdentityAccess.Admins.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

/// <summary>
///     Manage app-level administrators. Reserved for SuperAdmins via the
///     <see cref="Permission.AppRole.Manage" /> policy (only SuperAdmin holds it).
/// </summary>
[ApiController]
[Route("admins")]
[Produces("application/json")]
[Authorize(Policy = Permission.AppRole.Manage)]
public class AdminController(IMediator mediator) : ControllerBase
{
    /// <summary>
    ///     List users that hold an app-level role (Admin or Super Admin).
    /// </summary>
    [HttpGet(Name = "GetAdmins")]
    [ProducesResponseType(typeof(PagedResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdmins([FromQuery] GetAdminsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<UserDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    /// <summary>
    ///     Add an admin. Grants the Admin role to an existing user, or invites a new one by email.
    /// </summary>
    [HttpPost(Name = "AddAdmin")]
    [ProducesResponseType(typeof(AddAdminResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddAdmin([FromBody] AddAdminCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Revoke the Admin role from a user.
    /// </summary>
    [HttpDelete("{userId:guid}", Name = "RevokeAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevokeAdmin(Guid userId)
    {
        var result = await mediator.Send(new RevokeAdminCommand { UserId = userId });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     List pending admin invitations.
    /// </summary>
    [HttpGet("invitations", Name = "GetAdminInvitations")]
    [ProducesResponseType(typeof(PagedResponse<InvitationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminInvitations([FromQuery] GetAdminInvitationsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<InvitationDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    /// <summary>
    ///     Resend a pending admin invitation email.
    /// </summary>
    [HttpPost("invitations/{id:guid}/resend", Name = "ResendAdminInvitation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendAdminInvitation(Guid id)
    {
        var result = await mediator.Send(new ResendAdminInvitationCommand { Id = id });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Cancel a pending admin invitation.
    /// </summary>
    [HttpDelete("invitations/{id:guid}", Name = "CancelAdminInvitation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelAdminInvitation(Guid id)
    {
        var result = await mediator.Send(new CancelAdminInvitationCommand { Id = id });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
