using Logistics.Application.Tenant.Commands;
using Logistics.Application.Tenant.Queries;
using Logistics.Shared;
using Logistics.Shared.Models;
using Logistics.Shared.Consts.Policies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[Route("payrolls")]
[ApiController]
public class PayrollsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PayrollsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<PayrollDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payrolls.View)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _mediator.Send(new GetPayrollByIdQuery {Id = id});
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpGet("calculate")]
    [ProducesResponseType(typeof(Result<PayrollDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payrolls.View)]
    public async Task<IActionResult> CalculateEmployeePayroll([FromQuery] CalculatePayrollQuery request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PayrollDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payrolls.View)]
    public async Task<IActionResult> GetList([FromQuery] GetPayrollsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payrolls.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePayrollCommand request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payrolls.Edit)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePayrollCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payrolls.Delete)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _mediator.Send(new DeletePayrollCommand {Id = id});
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
