using Logistics.Application.Modules.Compliance.Ifta.TaxRates.Commands;
using Logistics.Application.Modules.Compliance.Ifta.TaxRates.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("ifta/rates")]
[Produces("application/json")]
public class IftaTaxRateController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetIftaTaxRates")]
    [ProducesResponseType(typeof(PagedResponse<IftaTaxRateDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.IftaRate.View)]
    public async Task<IActionResult> GetIftaTaxRates([FromQuery] GetIftaTaxRatesQuery request)
    {
        var result = await mediator.Send(request);
        return Ok(PagedResponse<IftaTaxRateDto>.FromPagedResult(result, request.Page, request.PageSize));
    }

    [HttpGet("{id:guid}", Name = "GetIftaTaxRateById")]
    [ProducesResponseType(typeof(IftaTaxRateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.IftaRate.View)]
    public async Task<IActionResult> GetIftaTaxRateById(Guid id)
    {
        var result = await mediator.Send(new GetIftaTaxRateQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpPost(Name = "CreateIftaTaxRate")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.IftaRate.Manage)]
    public async Task<IActionResult> CreateIftaTaxRate([FromBody] CreateIftaTaxRateCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess
            ? CreatedAtRoute("GetIftaTaxRateById", new { id = result.Value }, result.Value)
            : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateIftaTaxRate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.IftaRate.Manage)]
    public async Task<IActionResult> UpdateIftaTaxRate(Guid id, [FromBody] UpdateIftaTaxRateCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteIftaTaxRate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.IftaRate.Manage)]
    public async Task<IActionResult> DeleteIftaTaxRate(Guid id)
    {
        var result = await mediator.Send(new DeleteIftaTaxRateCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }
}
