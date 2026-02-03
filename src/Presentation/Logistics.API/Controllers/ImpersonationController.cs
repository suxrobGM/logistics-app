using Logistics.Application.Commands;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Logistics.API.Controllers;

[ApiController]
[Route("impersonation")]
[Produces("application/json")]
[EnableRateLimiting("impersonation")]
public class ImpersonationController(IMediator mediator) : ControllerBase
{
    [HttpPost(Name = "ImpersonateUser")]
    [ProducesResponseType(typeof(ImpersonateUserResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [Authorize(Policy = Permission.User.Manage)]
    public async Task<IActionResult> Impersonate([FromBody] ImpersonateUserCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }
}
