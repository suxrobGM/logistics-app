using Logistics.API.Extensions;
using Logistics.Application.Modules.Tenancy.Onboarding.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
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
    // Not Stat.View: that is held by Driver and Dispatcher too, and neither has any business
    // reading whether the company has set up payouts.
    [Authorize(Policy = Permission.Employee.Manage)]
    public async Task<IActionResult> GetOnboardingProgress([FromQuery] GetOnboardingProgressQuery request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }
}
