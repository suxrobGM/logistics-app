using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.Financial.Tax.Queries;

namespace Logistics.API.Controllers;

/// <summary>
///     Read-only tax metadata: supported jurisdictions for the manual-rate UI.
/// </summary>
[ApiController]
[Route("tax")]
[Produces("application/json")]
public class TaxController(IMediator mediator) : ControllerBase
{
    [HttpGet("jurisdictions", Name = "GetTaxJurisdictions")]
    [ProducesResponseType(typeof(IReadOnlyList<TaxJurisdictionInfoDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Tax.View)]
    public async Task<IActionResult> GetJurisdictions()
    {
        var result = await mediator.Send(new GetTaxJurisdictionsQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }
}
