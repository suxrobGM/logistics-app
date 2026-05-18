using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Logistics.Application.Modules.Financial.Tax.Commands;
using Logistics.Application.Modules.Financial.Tax.Queries;

namespace Logistics.API.Controllers;

/// <summary>
///     Tenant-managed tax rates used by <c>ManualTaxCalculator</c>. Owners and Managers can read;
///     only Owners can mutate (rates affect every customer invoice).
/// </summary>
[ApiController]
[Route("tax-rates")]
[Produces("application/json")]
public class TaxRatesController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetTenantTaxRates")]
    [ProducesResponseType(typeof(IReadOnlyList<TenantTaxRateDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Tax.View)]
    public async Task<IActionResult> GetList()
    {
        var result = await mediator.Send(new GetTenantTaxRatesQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost(Name = "CreateTenantTaxRate")]
    [ProducesResponseType(typeof(TenantTaxRateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Tax.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateTenantTaxRateCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateTenantTaxRate")]
    [ProducesResponseType(typeof(TenantTaxRateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Tax.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantTaxRateCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteTenantTaxRate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Tax.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteTenantTaxRateCommand(id));
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }
}
