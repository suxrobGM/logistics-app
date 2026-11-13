using Logistics.API.Extensions;
using Logistics.Application.Modules.Tenancy.ProductLicense.Commands;
using Logistics.Application.Modules.Tenancy.ProductLicense.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Logistics.API.Controllers.Tenancy;

[ApiController]
[Route("license")]
[Produces("application/json")]
public class ProductLicenseController(IMediator mediator) : ControllerBase
{
    public const string HeartbeatRateLimitPolicy = "license-heartbeat";

    [HttpGet(Name = "GetProductLicenseStatus")]
    [ProducesResponseType(typeof(ProductLicenseStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.ProductLicense.View)]
    public async Task<IActionResult> GetProductLicenseStatus()
    {
        var result = await mediator.Send(new GetProductLicenseStatusQuery());
        return result.ToActionResult();
    }

    [HttpPut("key", Name = "SetProductLicenseKey")]
    [ProducesResponseType(typeof(ProductLicenseStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.ProductLicense.Manage)]
    public async Task<IActionResult> SetProductLicenseKey([FromBody] SetProductLicenseKeyCommand command)
    {
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPost("heartbeat", Name = "RecordProductLicenseHeartbeat")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    [EnableRateLimiting(HeartbeatRateLimitPolicy)]
    public async Task<IActionResult> RecordProductLicenseHeartbeat([FromBody] ProductLicenseHeartbeatDto report)
    {
        var result = await mediator.Send(new RecordProductLicenseHeartbeatCommand(report));
        return result.ToActionResult();
    }

    /// <summary>
    /// Public discovery document. Served bare (no Result wrapper) because it is an external contract.
    /// </summary>
    [HttpGet("/.well-known/logisticsx.json", Name = "GetProductLicenseDiscovery")]
    [ProducesResponseType(typeof(ProductLicenseDiscoveryDto), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductLicenseDiscovery()
    {
        var result = await mediator.Send(new GetProductLicenseDiscoveryQuery());
        return result.ToActionResult();
    }
}
