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
public class AccountingController(IMediator mediator, IOptions<AccountingOptions> accountingOptions)
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

    /// <summary>
    ///     OAuth2 redirect target for QuickBooks. Intuit redirects the browser here (anonymous -
    ///     no app auth cookie/JWT), so the tenant is recovered from the signed <paramref name="state"/>.
    ///     Completes the connection and 302-redirects back to the TMS portal settings page.
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
        var returnUrl = accountingOptions.Value.QuickBooks?.PortalReturnUrl ?? "/";

        if (!string.IsNullOrEmpty(error) ||
            string.IsNullOrEmpty(code) || string.IsNullOrEmpty(realmId) || string.IsNullOrEmpty(state))
        {
            return Redirect(AppendStatus(returnUrl, "error"));
        }

        var result = await mediator.Send(new ConnectQuickBooksCommand
        {
            Code = code,
            RealmId = realmId,
            State = state
        });

        return Redirect(AppendStatus(returnUrl, result.IsSuccess ? "success" : "error"));
    }

    private static string AppendStatus(string url, string status)
    {
        var separator = url.Contains('?') ? '&' : '?';
        return $"{url}{separator}status={status}";
    }
}
