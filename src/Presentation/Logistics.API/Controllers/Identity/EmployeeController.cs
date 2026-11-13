using Logistics.API.Extensions;
using Logistics.Application.Modules.Financial.Payroll.Commands;
using Logistics.Application.Modules.Financial.Payroll.Queries;
using Logistics.Application.Modules.Identity.Employees.Commands;
using Logistics.Application.Modules.Identity.Employees.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers.Identity;

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
        return result.ToActionResult();
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
        return result.ToActionResult();
    }

    [HttpPut("{userId:guid}", Name = "UpdateEmployee")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Employee.Manage)]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateEmployeeCommand request)
    {
        request.UserId = userId;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpDelete("{userId:guid}", Name = "DeleteEmployee")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Employee.Manage)]
    public async Task<IActionResult> Delete(Guid userId)
    {
        var result = await mediator.Send(new DeleteEmployeeCommand { UserId = userId });
        return result.ToActionResult();
    }

    #region Driver licenses

    [HttpGet("{userId:guid}/licenses", Name = "GetDriverLicenses")]
    [ProducesResponseType(typeof(IList<DriverLicenseDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Employee.View)]
    public async Task<IActionResult> GetDriverLicenses(Guid userId, [FromQuery] bool includeRevoked = false)
    {
        var result = await mediator.Send(new GetDriverLicensesQuery
        {
            EmployeeId = userId,
            IncludeRevoked = includeRevoked
        });
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("{userId:guid}/licenses", Name = "CreateDriverLicense")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Employee.Manage)]
    public async Task<IActionResult> CreateDriverLicense(
        Guid userId, [FromBody] CreateDriverLicenseCommand request)
    {
        request.EmployeeId = userId;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPut("{userId:guid}/licenses/{licenseId:guid}", Name = "UpdateDriverLicense")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Employee.Manage)]
    public async Task<IActionResult> UpdateDriverLicense(
        Guid userId, Guid licenseId, [FromBody] UpdateDriverLicenseCommand request)
    {
        request.LicenseId = licenseId;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpDelete("{userId:guid}/licenses/{licenseId:guid}", Name = "DeleteDriverLicense")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Employee.Manage)]
    public async Task<IActionResult> DeleteDriverLicense(Guid userId, Guid licenseId)
    {
        var result = await mediator.Send(new DeleteDriverLicenseCommand { LicenseId = licenseId });
        return result.ToActionResult();
    }

    #endregion

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
        return result.ToActionResult();
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
        return result.ToActionResult();
    }

    #endregion

    #region Pay profile

    /// <summary>
    ///     Settlement settings for a driver: share-of-gross exclusions, fuel card deduction, recurring
    ///     deductions and the escrow balance.
    /// </summary>
    [HttpGet("{userId:guid}/payprofile", Name = "GetDriverPayProfile")]
    [ProducesResponseType(typeof(DriverPayProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Payroll.View)]
    public async Task<IActionResult> GetPayProfile(Guid userId)
    {
        var result = await mediator.Send(new GetDriverPayProfileQuery { EmployeeId = userId });
        return result.ToActionResult();
    }

    [HttpPut("{userId:guid}/payprofile", Name = "UpdateDriverPayProfile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Payroll.Manage)]
    public async Task<IActionResult> UpdatePayProfile(Guid userId, [FromBody] UpdateDriverPayProfileCommand request)
    {
        request.EmployeeId = userId;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    #endregion
}
