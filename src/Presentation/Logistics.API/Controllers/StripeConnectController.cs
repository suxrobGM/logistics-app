using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.Financial.StripeConnect.Commands;
using Logistics.Application.Modules.Financial.StripeConnect.Queries;

namespace Logistics.API.Controllers;

/// <summary>
/// API endpoints for Stripe Connect onboarding and management.
/// </summary>
[ApiController]
[Route("stripe/connect")]
[Produces("application/json")]
public class StripeConnectController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Creates a Stripe Connect Express account for the current tenant.
    /// </summary>
    [HttpPost("account", Name = "CreateConnectAccount")]
    [ProducesResponseType(typeof(CreateConnectAccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payment.Manage)]
    public async Task<IActionResult> CreateAccount()
    {
        var result = await mediator.Send(new CreateConnectAccountCommand());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Gets an onboarding link for the Stripe Connect account.
    /// The user should be redirected to this URL to complete onboarding.
    /// </summary>
    [HttpGet("onboarding-link", Name = "GetOnboardingLink")]
    [ProducesResponseType(typeof(OnboardingLinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payment.Manage)]
    public async Task<IActionResult> GetOnboardingLink(
        [FromQuery] string returnUrl,
        [FromQuery] string refreshUrl)
    {
        var result = await mediator.Send(new GetOnboardingLinkQuery(returnUrl, refreshUrl));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Refreshes the Stripe Connect status for the tenant: syncs from Stripe, persists the
    /// cached fields, and returns the latest status DTO. If the Stripe call fails the cached
    /// status is returned with a 200.
    /// </summary>
    [HttpPost("status/refresh", Name = "RefreshConnectStatus")]
    [ProducesResponseType(typeof(StripeConnectStatusDto), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Payment.View)]
    public async Task<IActionResult> RefreshStatus()
    {
        var result = await mediator.Send(new RefreshConnectStatusCommand());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Gets a login link for the Stripe Express dashboard.
    /// </summary>
    [HttpGet("dashboard-link", Name = "GetDashboardLink")]
    [ProducesResponseType(typeof(DashboardLinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payment.View)]
    public async Task<IActionResult> GetDashboardLink()
    {
        var result = await mediator.Send(new GetDashboardLinkQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }
}
