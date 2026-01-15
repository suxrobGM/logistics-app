using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

/// <summary>
/// Admin controller for managing customer portal users.
/// </summary>
[ApiController]
[Route("customer-users")]
[Produces("application/json")]
public class CustomerUserController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get a customer user by ID.
    /// </summary>
    [HttpGet("{id:guid}", Name = "GetCustomerUserById")]
    [ProducesResponseType(typeof(CustomerUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Customer.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetCustomerUserByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Get all customer users for a specific customer.
    /// </summary>
    [HttpGet("by-customer/{customerId:guid}", Name = "GetCustomerUsersByCustomer")]
    [ProducesResponseType(typeof(IEnumerable<CustomerUserDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Customer.View)]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        var result = await mediator.Send(new GetCustomerUsersByCustomerQuery { CustomerId = customerId });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Create a new customer portal user.
    /// </summary>
    [HttpPost(Name = "CreateCustomerUser")]
    [ProducesResponseType(typeof(CustomerUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Customer.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerUserCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Update a customer portal user.
    /// </summary>
    [HttpPut("{id:guid}", Name = "UpdateCustomerUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Customer.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerUserCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    /// Delete a customer portal user.
    /// </summary>
    [HttpDelete("{id:guid}", Name = "DeleteCustomerUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Customer.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteCustomerUserCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }
}
