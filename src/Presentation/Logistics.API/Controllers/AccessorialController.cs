using Logistics.Application.Modules.Financial.Accessorials.Commands;
using Logistics.Application.Modules.Financial.Accessorials.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

/// <summary>
/// The tenant's accessorial price list and default detention terms.
/// </summary>
[ApiController]
[Route("accessorials")]
[Produces("application/json")]
public class AccessorialController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetAccessorialCharges")]
    [ProducesResponseType(typeof(List<AccessorialChargeDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Load.View)]
    public async Task<IActionResult> GetList()
    {
        var result = await mediator.Send(new GetAccessorialChargesQuery());
        return Ok(result.Value);
    }

    [HttpPost(Name = "CreateAccessorialCharge")]
    [ProducesResponseType(typeof(AccessorialChargeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateAccessorialChargeCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateAccessorialCharge")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccessorialChargeCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteAccessorialCharge")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteAccessorialChargeCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet("detention", Name = "GetDetentionTerms")]
    [ProducesResponseType(typeof(DetentionTermsDto), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Load.View)]
    public async Task<IActionResult> GetDetentionTerms()
    {
        var result = await mediator.Send(new GetDetentionTermsQuery());
        return Ok(result.Value);
    }

    [HttpPut("detention", Name = "UpdateDetentionTerms")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Invoice.Manage)]
    public async Task<IActionResult> UpdateDetentionTerms([FromBody] UpdateDetentionTermsCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
