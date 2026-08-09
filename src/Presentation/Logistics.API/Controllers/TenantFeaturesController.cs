using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.IdentityAccess.Features.Commands;
using Logistics.Application.Modules.IdentityAccess.Features.Queries;

namespace Logistics.API.Controllers;

/// <summary>
/// Feature toggles for the calling user's own tenant. Admin-wide toggles live in <see cref="FeaturesController"/>.
/// </summary>
[ApiController]
[Route("tenants/features")]
[Produces("application/json")]
public class TenantFeaturesController(IMediator mediator) : ControllerBase
{
    // Every role needs this to render the UI, and it only ever returns the caller's own tenant.
    // Gating it on Tenant.View 403'd dispatchers and drivers, and the client's feature check fails
    // open - so disabled features silently stayed visible for exactly those roles.
    [HttpGet(Name = "GetCurrentTenantFeatures")]
    [ProducesResponseType(typeof(IReadOnlyList<FeatureStatusDto>), StatusCodes.Status200OK)]
    [Authorize]
    public async Task<IActionResult> GetFeatures()
    {
        var result = await mediator.Send(new GetTenantFeaturesQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Toggles one feature for the current tenant. Admin-locked features cannot be changed.
    /// </summary>
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

public record UpdateTenantFeatureRequest(bool IsEnabled);
