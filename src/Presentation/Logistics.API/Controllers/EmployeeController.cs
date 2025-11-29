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
public class EmployeeController(IMediator mediator) : ControllerBase
{
    [HttpGet("{userId:guid}", Name = "GetEmployeeById")]
    [ProducesResponseType(typeof(Result<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employees.View)]
    public async Task<IActionResult> GetById(Guid userId)
    {
        var result = await mediator.Send(new GetEmployeeByIdQuery { UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet(Name = "GetEmployees")]
    [ProducesResponseType(typeof(PagedResult<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employees.View)]
    public async Task<IActionResult> GetList([FromQuery] GetEmployeesQuery query)
    {
        var result = await mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost(Name = "CreateEmployee")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employees.Create)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeCommand request)
    {
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{userId:guid}/remove-role", Name = "RemoveRoleFromEmployee")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employees.Edit)]
    public async Task<IActionResult> RemoveRole(Guid userId, [FromBody] RemoveRoleFromEmployeeCommand request)
    {
        request.UserId = userId;
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{userId:guid}", Name = "UpdateEmployee")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employees.Edit)]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateEmployeeCommand request)
    {
        request.UserId = userId;
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{userId:guid}", Name = "DeleteEmployee")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employees.Delete)]
    public async Task<IActionResult> Delete(Guid userId)
    {
        var result = await mediator.Send(new DeleteEmployeeCommand { UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
