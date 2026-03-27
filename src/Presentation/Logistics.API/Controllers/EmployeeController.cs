using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CreateEmployeeCommand = Logistics.Application.Commands.CreateEmployeeCommand;
using UpdateEmployeeCommand = Logistics.Application.Commands.UpdateEmployeeCommand;

namespace Logistics.API.Controllers;

[ApiController]
[Route("employees")]
[Produces("application/json")]
public class EmployeeController(IMediator mediator) : ControllerBase
{
    [HttpGet("{userId:guid}", Name = "GetEmployeeById")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Employee.View)]
    public async Task<IActionResult> GetById(Guid userId)
    {
        var result = await mediator.Send(new GetEmployeeByIdQuery { UserId = userId });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetEmployees")]
    [ProducesResponseType(typeof(PagedResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Employee.View)]
    public async Task<IActionResult> GetList([FromQuery] GetEmployeesQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<EmployeeDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpPost(Name = "CreateEmployee")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Employee.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{userId:guid}", Name = "UpdateEmployee")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Employee.Manage)]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateEmployeeCommand request)
    {
        request.UserId = userId;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{userId:guid}", Name = "DeleteEmployee")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Employee.Manage)]
    public async Task<IActionResult> Delete(Guid userId)
    {
        var result = await mediator.Send(new DeleteEmployeeCommand { UserId = userId });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    #region Payout

    /// <summary>
    ///     Set up a Stripe Connect Express account for employee payroll payouts.
    /// </summary>
    [HttpPost("{userId:guid}/payout/setup", Name = "SetupEmployeePayout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Employee.Manage)]
    public async Task<IActionResult> SetupEmployeePayout(Guid userId)
    {
        var result = await mediator.Send(new SetupEmployeePayoutCommand { EmployeeId = userId });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Get the Stripe onboarding link for an employee to complete their payout account setup.
    /// </summary>
    [HttpGet("{userId:guid}/payout/onboarding-link", Name = "GetEmployeePayoutOnboardingLink")]
    [ProducesResponseType(typeof(EmployeePayoutOnboardingLinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Employee.Manage)]
    public async Task<IActionResult> GetEmployeePayoutOnboardingLink(
        Guid userId, [FromQuery] string returnUrl, [FromQuery] string refreshUrl)
    {
        var result = await mediator.Send(new GetEmployeePayoutOnboardingLinkQuery
        {
            EmployeeId = userId,
            ReturnUrl = returnUrl,
            RefreshUrl = refreshUrl
        });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion
}
