using Logistics.Shared.Models;
using Logistics.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.IdentityAccess.Features.Queries;

namespace Logistics.API.Controllers;

/// <summary>
/// Read-only feature state for the calling user's own tenant. Only admins change features, through <see cref="FeaturesController"/>.
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
}
