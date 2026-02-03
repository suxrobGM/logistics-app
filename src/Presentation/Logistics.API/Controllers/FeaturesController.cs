using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Application.Services;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

/// <summary>
/// API controller for managing feature toggles (admin operations).
/// </summary>
[ApiController]
[Route("features")]
[Produces("application/json")]
public class FeaturesController(IMediator mediator) : ControllerBase
{
    #region Default Features

    /// <summary>
    /// Gets the default feature configuration for new tenants.
    /// </summary>
    [HttpGet("defaults", Name = "GetDefaultFeatures")]
    [ProducesResponseType(typeof(IReadOnlyList<DefaultFeatureStatusDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Tenant.Manage)]
    public async Task<IActionResult> GetDefaultFeatures()
    {
        var result = await mediator.Send(new GetDefaultFeaturesQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Updates the default feature configuration for new tenants.
    /// </summary>
    [HttpPut("defaults", Name = "UpdateDefaultFeatures")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Tenant.Manage)]
    public async Task<IActionResult> UpdateDefaultFeatures([FromBody] UpdateDefaultFeaturesCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Tenant Features

    /// <summary>
    /// Gets the feature configuration for a specific tenant.
    /// </summary>
    [HttpGet("tenant/{tenantId:guid}", Name = "GetTenantFeatures")]
    [ProducesResponseType(typeof(IReadOnlyList<FeatureStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Tenant.Manage)]
    public async Task<IActionResult> GetTenantFeatures(Guid tenantId)
    {
        var result = await mediator.Send(new GetTenantFeaturesQuery { TenantId = tenantId });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Updates the feature configuration for a specific tenant.
    /// </summary>
    [HttpPut("tenant/{tenantId:guid}", Name = "UpdateTenantFeatures")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Tenant.Manage)]
    public async Task<IActionResult> UpdateTenantFeatures(Guid tenantId, [FromBody] UpdateTenantFeaturesAdminCommand request)
    {
        request.TenantId = tenantId;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion
}
