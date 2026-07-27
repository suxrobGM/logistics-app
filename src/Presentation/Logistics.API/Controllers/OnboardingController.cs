using Logistics.Application.Modules.Platform.Onboarding.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("onboarding")]
[Produces("application/json")]
public class OnboardingController(IMediator mediator) : ControllerBase
{
    [HttpGet("progress", Name = "GetOnboardingProgress")]
    [ProducesResponseType(typeof(OnboardingProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Stat.View)]
    public async Task<IActionResult> GetOnboardingProgress([FromQuery] GetOnboardingProgressQuery request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }
}
