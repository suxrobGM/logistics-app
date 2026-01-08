using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("customers")]
public class CustomerController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "GetCustomerById")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permissions.Customers.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetCustomerByIdQuery { Id = id });
        return result.Success ? Ok(result.Data) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetCustomers")]
    [ProducesResponseType(typeof(PagedResponse<CustomerDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permissions.Customers.View)]
    public async Task<IActionResult> GetList([FromQuery] GetCustomersQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<CustomerDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpPost(Name = "CreateCustomer")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Customers.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand request)
    {
        var result = await mediator.Send(request);
        return result.Success ? Ok(result.Data) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateCustomer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Customers.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.Success ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteCustomer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permissions.Customers.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteCustomerCommand { Id = id });
        return result.Success ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }
}
