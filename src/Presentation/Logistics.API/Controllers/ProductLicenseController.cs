using Logistics.Application.Modules.Platform.ProductLicense.Commands;
using Logistics.Application.Modules.Platform.ProductLicense.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Logistics.API.Controllers;

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
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("key", Name = "SetProductLicenseKey")]
    [ProducesResponseType(typeof(ProductLicenseStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.ProductLicense.Manage)]
    public async Task<IActionResult> SetProductLicenseKey([FromBody] SetProductLicenseKeyCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("heartbeat", Name = "RecordProductLicenseHeartbeat")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    [EnableRateLimiting(HeartbeatRateLimitPolicy)]
    public async Task<IActionResult> RecordProductLicenseHeartbeat([FromBody] RecordProductLicenseHeartbeatCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
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
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }
}
