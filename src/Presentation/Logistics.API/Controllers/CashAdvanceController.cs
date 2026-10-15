using Logistics.Application.Modules.Financial.Payroll.Commands;
using Logistics.Application.Modules.Financial.Payroll.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

/// <summary>
/// Cash advances to drivers. Approved advances are deducted from the driver's next settlement.
/// </summary>
[ApiController]
[Route("cashadvances")]
[Produces("application/json")]
public class CashAdvanceController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetCashAdvances")]
    [ProducesResponseType(typeof(List<CashAdvanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.View)]
    public async Task<IActionResult> GetList([FromQuery] GetCashAdvancesQuery query)
    {
        var result = await mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost(Name = "IssueCashAdvance")]
    [ProducesResponseType(typeof(CashAdvanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> Issue([FromBody] IssueCashAdvanceCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("{id:guid}/approve", Name = "ApproveCashAdvance")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await mediator.Send(new ReviewCashAdvanceCommand { Id = id, Approve = true });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("{id:guid}/reject", Name = "RejectCashAdvance")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> Reject(Guid id)
    {
        var result = await mediator.Send(new ReviewCashAdvanceCommand { Id = id, Approve = false });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteCashAdvance")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteCashAdvanceCommand { Id = id });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
