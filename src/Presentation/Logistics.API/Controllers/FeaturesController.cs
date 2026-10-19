using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Logistics.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.IdentityAccess.Features.Commands;
using Logistics.Application.Modules.IdentityAccess.Features.Queries;

namespace Logistics.API.Controllers;

/// <summary>
/// API controller for managing feature toggles (admin operations).
/// </summary>
[ApiController]
[Route("features")]
[Produces("application/json")]
public class FeaturesController(IMediator mediator) : ControllerBase
{
    #region Tenant Features

    /// <summary>
    /// Gets the feature configuration for a specific tenant.
    /// </summary>
    [HttpGet("tenant/{tenantId:guid}", Name = "GetTenantFeatures")]
    [ProducesResponseType(typeof(IReadOnlyList<FeatureStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
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
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> UpdateTenantFeatures(Guid tenantId, [FromBody] UpdateTenantFeaturesAdminCommand request)
    {
        request.TenantId = tenantId;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion
}
