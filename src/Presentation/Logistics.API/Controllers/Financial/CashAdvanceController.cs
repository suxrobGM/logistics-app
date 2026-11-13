using Logistics.API.Extensions;
using Logistics.Application.Modules.Financial.Payroll.Commands;
using Logistics.Application.Modules.Financial.Payroll.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers.Financial;

/// <summary>
/// Cash advances to drivers, deducted from the driver's next settlement.
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
        return result.ToActionResult();
    }

    [HttpPost(Name = "IssueCashAdvance")]
    [ProducesResponseType(typeof(CashAdvanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> Issue([FromBody] IssueCashAdvanceCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}", Name = "DeleteCashAdvance")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteCashAdvanceCommand { Id = id });
        return result.ToActionResult();
    }
}
