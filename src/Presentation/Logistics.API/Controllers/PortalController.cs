using Logistics.API.Extensions;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Portal;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

/// <summary>
///     API endpoints for the customer portal.
///     Customers can view their loads, invoices, and documents.
/// </summary>
[ApiController]
[Route("portal")]
[Produces("application/json")]
[Authorize(Policy = Permission.Portal.Access)]
public class PortalController(IMediator mediator) : ControllerBase
{
    /// <summary>
    ///     Get the current customer user's profile.
    /// </summary>
    [HttpGet("me", Name = "GetPortalProfile")]
    [ProducesResponseType(typeof(CustomerUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return NotFound(new ErrorResponse("User not authenticated."));
        }

        var result = await mediator.Send(new GetCustomerUserQuery { UserId = userId.Value });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Get loads for the authenticated customer.
    /// </summary>
    [HttpGet("loads", Name = "GetPortalLoads")]
    [ProducesResponseType(typeof(PagedResponse<PortalLoadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Portal.ViewLoads)]
    public async Task<IActionResult> GetLoads([FromQuery] GetPortalLoadsRequest request)
    {
        var customerId = await GetCustomerIdAsync();
        if (customerId is null)
        {
            return BadRequest(new ErrorResponse("Customer user not found."));
        }

        var query = new GetPortalLoadsQuery
        {
            CustomerId = customerId.Value,
            Search = request.Search,
            Page = request.Page,
            PageSize = request.PageSize,
            OrderBy = request.OrderBy,
            OnlyActiveLoads = request.OnlyActiveLoads,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var result = await mediator.Send(query);
        return Ok(PagedResponse<PortalLoadDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    /// <summary>
    ///     Get a specific load by ID.
    /// </summary>
    [HttpGet("loads/{loadId:guid}", Name = "GetPortalLoad")]
    [ProducesResponseType(typeof(PortalLoadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Portal.ViewLoads)]
    public async Task<IActionResult> GetLoad(Guid loadId)
    {
        var customerId = await GetCustomerIdAsync();
        if (customerId is null)
        {
            return NotFound(new ErrorResponse("Customer user not found."));
        }

        var result = await mediator.Send(new GetPortalLoadQuery
        {
            LoadId = loadId,
            CustomerId = customerId.Value
        });

        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Get documents for a specific load.
    /// </summary>
    [HttpGet("loads/{loadId:guid}/documents", Name = "GetPortalLoadDocuments")]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Portal.ViewDocuments)]
    public async Task<IActionResult> GetLoadDocuments(Guid loadId)
    {
        var customerId = await GetCustomerIdAsync();
        if (customerId is null)
        {
            return NotFound(new ErrorResponse("Customer user not found."));
        }

        var result = await mediator.Send(new GetPortalLoadDocumentsQuery
        {
            LoadId = loadId,
            CustomerId = customerId.Value
        });

        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Get invoices for the authenticated customer.
    /// </summary>
    [HttpGet("invoices", Name = "GetPortalInvoices")]
    [ProducesResponseType(typeof(PagedResponse<PortalInvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Portal.ViewInvoices)]
    public async Task<IActionResult> GetInvoices([FromQuery] GetPortalInvoicesRequest request)
    {
        var customerId = await GetCustomerIdAsync();
        if (customerId is null)
        {
            return BadRequest(new ErrorResponse("Customer user not found."));
        }

        var query = new GetPortalInvoicesQuery
        {
            CustomerId = customerId.Value,
            Search = request.Search,
            Page = request.Page,
            PageSize = request.PageSize,
            OrderBy = request.OrderBy,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var result = await mediator.Send(query);
        return Ok(PagedResponse<PortalInvoiceDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    private async Task<Guid?> GetCustomerIdAsync()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return null;
        }

        var result = await mediator.Send(new GetCustomerUserQuery { UserId = userId.Value });
        return result.IsSuccess ? result.Value?.CustomerId : null;
    }
}
