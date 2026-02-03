using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Application.Services;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

/// <summary>
/// API controller for tenant-specific feature toggles (tenant owner operations).
/// </summary>
[ApiController]
[Route("tenantfeatures")]
[Produces("application/json")]
public class TenantFeaturesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Gets all feature statuses for the current tenant.
    /// </summary>
    [HttpGet(Name = "GetCurrentTenantFeatures")]
    [ProducesResponseType(typeof(IReadOnlyList<FeatureStatusDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Tenant.View)]
    public async Task<IActionResult> GetFeatures()
    {
        var result = await mediator.Send(new GetTenantFeaturesQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Toggles a specific feature for the current tenant.
    /// </summary>
    /// <remarks>
    /// This endpoint is used by tenant owners to enable/disable features.
    /// Features locked by admin cannot be changed.
    /// </remarks>
    [HttpPut("{feature}", Name = "UpdateCurrentTenantFeature")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Tenant.Manage)]
    public async Task<IActionResult> UpdateFeature(TenantFeature feature, [FromBody] UpdateTenantFeatureRequest request)
    {
        var result = await mediator.Send(new UpdateTenantFeatureCommand
        {
            Feature = feature,
            IsEnabled = request.IsEnabled
        });

        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}

/// <summary>
/// Request model for updating a tenant feature.
/// </summary>
public record UpdateTenantFeatureRequest(bool IsEnabled);
