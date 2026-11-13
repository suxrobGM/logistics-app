using Logistics.API.Extensions;
using Logistics.Application.Modules.Financial.Accessorials.Commands;
using Logistics.Application.Modules.Financial.Accessorials.Queries;
using Logistics.Application.Modules.Operations.Customers.Commands;
using Logistics.Application.Modules.Operations.Customers.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers.Operations;

[ApiController]
[Route("customers")]
[Produces("application/json")]
public class CustomerController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "GetCustomerById")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Customer.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetCustomerByIdQuery { Id = id });
        return result.ToActionResult();
    }

    [HttpGet(Name = "GetCustomers")]
    [ProducesResponseType(typeof(PagedResponse<CustomerDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Customer.View)]
    public async Task<IActionResult> GetList([FromQuery] GetCustomersQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<CustomerDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpPost(Name = "CreateCustomer")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Customer.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}", Name = "UpdateCustomer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Customer.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}", Name = "DeleteCustomer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Customer.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteCustomerCommand { Id = id });
        return result.ToActionResult();
    }

    /// <summary>
    ///     The customer's own detention terms; null fields use the company default.
    /// </summary>
    [HttpGet("{id:guid}/detention", Name = "GetCustomerDetentionTerms")]
    [ProducesResponseType(typeof(DetentionTermsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Customer.View)]
    public async Task<IActionResult> GetDetentionTerms(Guid id)
    {
        var result = await mediator.Send(new GetCustomerDetentionTermsQuery { CustomerId = id });
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}/detention", Name = "UpdateCustomerDetentionTerms")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Customer.Manage)]
    public async Task<IActionResult> UpdateDetentionTerms(Guid id, [FromBody] UpdateCustomerDetentionTermsCommand request)
    {
        request.CustomerId = id;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}/preferences", Name = "SetCustomerUpdatePreferences")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Customer.Manage)]
    public async Task<IActionResult> SetUpdatePreferences(Guid id, [FromBody] SetCustomerUpdatePreferencesCommand request)
    {
        request.CustomerId = id;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }
}
