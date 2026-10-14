using Logistics.Application.Modules.Integrations.Accounting.Commands;
using Logistics.Application.Modules.Integrations.Accounting.Queries;
using Logistics.Infrastructure.Integrations.Accounting;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Logistics.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Logistics.API.Controllers;

[ApiController]
[Route("accounting")]
[Produces("application/json")]
public class AccountingController(
    IMediator mediator,
    IOptions<AccountingOptions> accountingOptions,
    ILogger<AccountingController> logger)
    : ControllerBase
{
    [HttpGet("quickbooks", Name = "GetQuickBooksConnection")]
    [ProducesResponseType(typeof(AccountingConnectionDto), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Accounting.View)]
    public async Task<IActionResult> GetConnection()
    {
        var result = await mediator.Send(new GetQuickBooksConnectionQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("quickbooks/connect", Name = "ConnectQuickBooks")]
    [ProducesResponseType(typeof(AccountingAuthUrlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Accounting.Manage)]
    public async Task<IActionResult> Connect()
    {
        var result = await mediator.Send(new GetQuickBooksAuthUrlQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("quickbooks", Name = "DisconnectQuickBooks")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Accounting.Manage)]
    public async Task<IActionResult> Disconnect()
    {
        var result = await mediator.Send(new DisconnectQuickBooksCommand());
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("quickbooks/lookups", Name = "GetQuickBooksLookups")]
    [ProducesResponseType(typeof(AccountingLookupsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Accounting.Manage)]
    public async Task<IActionResult> GetLookups()
    {
        var result = await mediator.Send(new GetQuickBooksLookupsQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("quickbooks/settings", Name = "UpdateQuickBooksSettings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Accounting.Manage)]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateQuickBooksSettingsCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("quickbooks/sync", Name = "SyncQuickBooks")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Accounting.Manage)]
    public async Task<IActionResult> Sync()
    {
        var result = await mediator.Send(new SyncQuickBooksCommand());
        return result.IsSuccess ? Accepted() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("quickbooks/issues", Name = "GetQuickBooksSyncIssues")]
    [ProducesResponseType(typeof(List<AccountingSyncIssueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Accounting.View)]
    public async Task<IActionResult> GetSyncIssues()
    {
        var result = await mediator.Send(new GetQuickBooksSyncIssuesQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("quickbooks/issues/retry", Name = "RetryQuickBooksSyncIssues")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Accounting.Manage)]
    public async Task<IActionResult> RetrySyncIssues([FromBody] RetryQuickBooksSyncIssuesCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Accepted() : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     OAuth2 redirect target for QuickBooks. Intuit redirects the browser here (anonymous -
    ///     no app auth cookie/JWT), so the tenant is recovered from the signed <paramref name="state"/>.
    ///     Always ends in a 302 back to the TMS portal with <c>status=success|error</c>; the user never
    ///     sees an API error page.
    /// </summary>
    [HttpGet("quickbooks/callback", Name = "QuickBooksCallback")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? realmId,
        [FromQuery] string? state,
        [FromQuery] string? error)
    {
        if (!string.IsNullOrEmpty(error) ||
            string.IsNullOrEmpty(code) || string.IsNullOrEmpty(realmId) || string.IsNullOrEmpty(state))
        {
            return RedirectToPortal(string.Equals(error, "access_denied", StringComparison.Ordinal) ? "denied" : "error");
        }

        try
        {
            var result = await mediator.Send(new ConnectQuickBooksCommand
            {
                Code = code,
                RealmId = realmId,
                State = state
            });
            return RedirectToPortal(result.IsSuccess ? "success" : "error");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QuickBooks OAuth callback failed for realm {RealmId}", realmId);
            return RedirectToPortal("error");
        }
    }

    /// <summary>
    ///     Disconnect landing page (the "Disconnect URL" in the Intuit app settings): Intuit sends the
    ///     browser here after the user disconnects the app from inside QuickBooks. Anonymous and carries
    ///     nothing we can trust, so it changes no state - the next sync sees the revoked token and flags
    ///     the connection for reconnect.
    /// </summary>
    [HttpGet("quickbooks/disconnected", Name = "QuickBooksDisconnected")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Disconnected() => RedirectToPortal("disconnected");

    private RedirectResult RedirectToPortal(string status)
    {
        var url = accountingOptions.Value.QuickBooks?.PortalReturnUrl ?? "/";
        var separator = url.Contains('?') ? '&' : '?';
        return Redirect($"{url}{separator}status={status}");
    }
}
